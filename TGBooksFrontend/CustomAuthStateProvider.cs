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
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private AuthenticationState? _cachedState;

        public CustomAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_cachedState != null)
            {
                return _cachedState;
            }

            try
            {
                var json = await _jsRuntime.InvokeAsync<string?>(
                    "localStorage.getItem",
                    "tgbooks_user_session");

                if (string.IsNullOrEmpty(json))
                {
                    _cachedState = new AuthenticationState(_anonymous);
                    return _cachedState;
                }

                var session = JsonSerializer.Deserialize<UserSession>(json);

                if (session?.JwtToken == null)
                {
                    _cachedState = new AuthenticationState(_anonymous);
                    return _cachedState;
                }

                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, session.Id.ToString()),
                    new Claim(ClaimTypes.Name, $"{session.FirstName} {session.LastName}".Trim()),
                    new Claim(ClaimTypes.Email, session.Email ?? ""),
                    new Claim(ClaimTypes.Role, session.Role ?? ""),
                    new Claim("JwtToken", session.JwtToken) // Store token in claims
                }, "JwtAuth");

                _cachedState = new AuthenticationState(new ClaimsPrincipal(identity));
                return _cachedState;
            }
            catch
            {
                _cachedState = new AuthenticationState(_anonymous);
                return _cachedState;
            }
        }

        public UserSession? GetCurrentUserSession()
        {
            if (_cachedState == null || !_cachedState.User.Identity?.IsAuthenticated == true)
            {
                return null;
            }

            var user = _cachedState.User;
            var nameClaim = user.FindFirst(ClaimTypes.Name)?.Value ?? "";
            var nameParts = nameClaim.Split(' ', 2);

            return new UserSession
            {
                Id = int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedId) ? parsedId : 0,
                FirstName = nameParts.Length > 0 ? nameParts[0] : "",
                LastName = nameParts.Length > 1 ? nameParts[1] : "",
                Email = user.FindFirst(ClaimTypes.Email)?.Value,
                Role = user.FindFirst(ClaimTypes.Role)?.Value,
                JwtToken = user.FindFirst("JwtToken")?.Value // Safely populates token now!
            };
        }

        public async Task MarkUserAsAuthenticated(UserSession session)
        {
            var json = JsonSerializer.Serialize(session);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "tgbooks_user_session", json);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, session.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{session.FirstName} {session.LastName}".Trim()),
                new Claim(ClaimTypes.Email, session.Email ?? ""),
                new Claim(ClaimTypes.Role, session.Role ?? ""),
                new Claim("JwtToken", session.JwtToken ?? "")
            }, "JwtAuth");

            var user = new ClaimsPrincipal(identity);
            _cachedState = new AuthenticationState(user);

            NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "tgbooks_user_session");
            _cachedState = new AuthenticationState(_anonymous);

            NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
        }
    }
}