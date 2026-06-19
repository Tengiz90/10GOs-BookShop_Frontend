using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TGBooksFrontend.Services;

namespace TGBooksFrontend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddTransient<BlazorAuthorizationHandler>();

            builder.Services.AddHttpClient("SecureBackendClient", client =>
            {
                client.BaseAddress = new Uri("https://tg-books-backend-gyfhgbaye3evbpek.polandcentral-01.azurewebsites.net/api/");
            })
            .AddHttpMessageHandler<BlazorAuthorizationHandler>();

            builder.Services.AddScoped<CartService>();

            builder.Services.AddScoped(sp =>
                sp.GetRequiredService<IHttpClientFactory>().CreateClient("SecureBackendClient"));

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

            await builder.Build().RunAsync();
        }
    }
}