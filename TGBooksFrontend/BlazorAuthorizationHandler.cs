using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;

namespace TGBooksFrontend
{
    public class BlazorAuthorizationHandler : DelegatingHandler
    {
        private readonly AuthenticationStateProvider _authStateProvider;

        public BlazorAuthorizationHandler(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. Cast and look for an active in-memory user session
            if (_authStateProvider is CustomAuthStateProvider customProvider)
            {
                var session = customProvider.GetCurrentUserSession();

                // 2. Fall back to reading local storage via GetAuthenticationStateAsync if memory is uninitialized
                if (session == null)
                {
                    await customProvider.GetAuthenticationStateAsync();
                    session = customProvider.GetCurrentUserSession();
                }

                // 3. If a token exists, attach it seamlessly to the authorization headers
                if (session != null && !string.IsNullOrEmpty(session.JwtToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.JwtToken);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}