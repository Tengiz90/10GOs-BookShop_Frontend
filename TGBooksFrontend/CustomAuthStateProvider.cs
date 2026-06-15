using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection; // Add this namespace for GetService
using Microsoft.JSInterop;
using TGBooksFrontend.Models;

namespace TGBooksFrontend
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IServiceProvider _serviceProvider; // Inject this instead of HttpClient directly
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private UserSession? _currentUserSession;

        // Change constructor to receive IServiceProvider
        public CustomAuthStateProvider(IJSRuntime jsRuntime, IServiceProvider serviceProvider)
        {
            _jsRuntime = jsRuntime;
            _serviceProvider = serviceProvider;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var storedSessionJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "tgbooks_user_session");

                if (string.IsNullOrEmpty(storedSessionJson))
                {
                    return new AuthenticationState(_anonymous);
                }

                var session = JsonSerializer.Deserialize<UserSession>(storedSessionJson);
                if (session == null || string.IsNullOrEmpty(session.JwtToken))
                {
                    return new AuthenticationState(_anonymous);
                }

                _currentUserSession = session;
                return BuildStatePrincipal(session);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public UserSession? GetCurrentUserSession() => _currentUserSession;

        public async Task MarkUserAsAuthenticated(UserSession session)
        {
            _currentUserSession = session;

            var sessionJson = JsonSerializer.Serialize(session);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "tgbooks_user_session", sessionJson);

            // Resolve HttpClient dynamically out of the active scope container
            var http = _serviceProvider.GetRequiredService<HttpClient>();
            if (http != null)
            {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.JwtToken);
            }

            var authState = BuildStatePrincipal(session);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        public async Task MarkUserAsLoggedOut()
        {
            _currentUserSession = null;

            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "tgbooks_user_session");

            // Resolve HttpClient dynamically to drop headers without blocking application startup
            var http = _serviceProvider.GetRequiredService<HttpClient>();
            if (http != null)
            {
                http.DefaultRequestHeaders.Authorization = null;
            }

            var authState = new AuthenticationState(_anonymous);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        private AuthenticationState BuildStatePrincipal(UserSession session)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, session.Id.ToString()),
                new Claim(ClaimTypes.Name, session.FullName),
                new Claim(ClaimTypes.Email, session.Email),
                new Claim(ClaimTypes.Role, session.Role)
            }, "JwtAuth");

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}