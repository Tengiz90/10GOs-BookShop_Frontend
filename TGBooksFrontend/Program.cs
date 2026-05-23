using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace TGBooksFrontend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // 1. Register the custom authorization handler first
            builder.Services.AddTransient<BlazorAuthorizationHandler>();

            // 2. Configure the HttpClient pipeline using the factory pattern to attach the handler
            builder.Services.AddHttpClient("SecureBackendClient", client =>
            {
                client.BaseAddress = new Uri("https://tg-books-backend-gyfhgbaye3evbpek.polandcentral-01.azurewebsites.net/api/");
            })
            .AddHttpMessageHandler<BlazorAuthorizationHandler>();

            // 3. Register the token-aware client as the default HttpClient for your components
            builder.Services.AddScoped(sp =>
                sp.GetRequiredService<IHttpClientFactory>().CreateClient("SecureBackendClient"));

            // 4. Setup authentication and your custom provider
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

            await builder.Build().RunAsync();
        }
    }
}