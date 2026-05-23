using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using TGBooksFrontend.Models;

namespace TGBooksFrontend
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private UserSession? _currentUserSession;

        public CustomAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Pull stored session info directly from browser storage on load
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

            // Serialize and commit session data string to local storage cache
            var sessionJson = JsonSerializer.Serialize(session);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "tgbooks_user_session", sessionJson);

            var authState = BuildStatePrincipal(session);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        public async Task MarkUserAsLoggedOut()
        {
            _currentUserSession = null;

            // Wipe token data entirely out of local storage
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "tgbooks_user_session");

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