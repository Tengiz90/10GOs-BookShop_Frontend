using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using TGBooksFrontend.Models;

namespace TGBooksFrontend.Services
{
    public class CartService
    {
        private readonly HttpClient _http;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigation;
        private readonly IJSRuntime _js;

        public CartService(
            HttpClient http,
            AuthenticationStateProvider authStateProvider,
            NavigationManager navigation,
            IJSRuntime js)
        {
            _http = http;
            _authStateProvider = authStateProvider;
            _navigation = navigation;
            _js = js;
        }

        public async Task AddToCartAsync(Book book)
        {
            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity == null || !user.Identity.IsAuthenticated)
                {
                    _navigation.NavigateTo("login");
                    return;
                }

                var cartRequest = new AddCartItem
                {
                    BookId = book.Id,
                    Quantity = 1
                };

                var httpResponse = await _http.PostAsJsonAsync("users/cart", cartRequest);

                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized ||
                    httpResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    _navigation.NavigateTo("login");
                    return;
                }

                var apiResponse = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetCartItem?>>();

                if (httpResponse.IsSuccessStatusCode && apiResponse != null && apiResponse.WasSuccessful)
                {
                    book.AlreadyInCart = true;
                    Console.WriteLine($"Added {book.Title} to cart successfully.");
                }
                else
                {
                    string errorMsg = apiResponse?.Message ?? "An error occurred while adding the item.";
                    await _js.InvokeVoidAsync("alert", errorMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to cart processing: {ex.Message}");
                await _js.InvokeVoidAsync("alert", "A critical error occurred.");
            }
        }

        public async Task<(List<GetCartItem> Items, string? ErrorMessage)> GetCartAsync()
        {
            try
            {
                var httpResponse = await _http.GetAsync("users/cart");

                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized ||
                    httpResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    _navigation.NavigateTo("login");
                    return (new List<GetCartItem>(), null);
                }

                var response = await httpResponse.Content
                    .ReadFromJsonAsync<ApiResponse<ICollection<GetCartItem>>>();

                if (httpResponse.IsSuccessStatusCode && response != null && response.WasSuccessful)
                {
                    return (response.Data?.ToList() ?? new List<GetCartItem>(), null);
                }

                return (
                    new List<GetCartItem>(),
                    response?.Message ?? "Failed to securely load cart allocations."
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cart Fetch Crash: {ex.Message}");
                return (new List<GetCartItem>(), "Unable to connect to system service records.");
            }
        }

        public async Task<(GetCartItem? UpdatedItem, string Message, bool Success)> UpdateItemQuantityAsync(
            int cartItemId,
            int newQuantity)
        {
            if (newQuantity < 1)
            {
                return (null, "Quantity cannot be less than 1.", false);
            }

            try
            {
                var payload = new ChangeCartItemQuantity
                {
                    CartItemId = cartItemId,
                    Quantity = newQuantity
                };

                var httpResponse = await _http.PutAsJsonAsync("users/cart", payload);

                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized ||
                    httpResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    _navigation.NavigateTo("login");
                    return (null, "", false);
                }

                var apiResponse = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<GetCartItem>>();

                if (httpResponse.IsSuccessStatusCode && apiResponse != null && apiResponse.WasSuccessful)
                {
                    return (
                        apiResponse.Data,
                        "Cart distribution amounts balanced.",
                        true
                    );
                }

                if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    return (
                        null,
                        apiResponse?.Message ?? "Stock capacity limit exceeded.",
                        false
                    );
                }

                return (
                    null,
                    apiResponse?.Message ?? "Server returned an error updated stock records.",
                    false
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Quantity Mod Fault: {ex.Message}");
                return (
                    null,
                    "Failed to synchronize adjustments with server counters.",
                    false
                );
            }
        }

        public async Task<(bool Success, string Message)> RemoveItemFromCartAsync(int cartItemId)
        {
            try
            {
                var payload = new RemoveCartItem
                {
                    CartItemId = cartItemId
                };

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(_http.BaseAddress!, "users/cart"),
                    Content = JsonContent.Create(payload)
                };

                var httpResponse = await _http.SendAsync(request);

                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized ||
                    httpResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    _navigation.NavigateTo("login");
                    return (false, "");
                }

                var apiResponse = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<int?>>();

                if (httpResponse.IsSuccessStatusCode && apiResponse != null && apiResponse.WasSuccessful)
                {
                    return (true, "Item removed successfully from your tracking allocation.");
                }

                return (
                    false,
                    apiResponse?.Message ?? "Could not purge item from allocation list."
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Item Deletion Error: {ex.Message}");
                return (false, "Failed to complete item purge operation.");
            }
        }

        public void ProceedToCheckout()
        {
            
        }

        private class ApiResponse<T>
        {
            public bool WasSuccessful { get; set; }
            public string Message { get; set; } = string.Empty;
            public T? Data { get; set; }
        }
    }
}