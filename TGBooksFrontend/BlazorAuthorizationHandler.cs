using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Text.Json;
using TGBooksFrontend.Models;

public class BlazorAuthorizationHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;

    public BlazorAuthorizationHandler(IJSRuntime js)
    {
        _js = js;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var json = await _js.InvokeAsync<string?>(
            "localStorage.getItem",
            "tgbooks_user_session");

        if (!string.IsNullOrEmpty(json))
        {
            var session = JsonSerializer.Deserialize<UserSession>(json);

            if (session?.JwtToken != null)
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", session.JwtToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}