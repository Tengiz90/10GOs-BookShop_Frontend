using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TGBooksFrontend.Models;

namespace TGBooksFrontend.Services
{
    public class CartService
    {
        private readonly HttpClient _http;
        private readonly NavigationManager _navigation;
        private readonly IJSRuntime _js;

        public CartService(
            HttpClient http,
            NavigationManager navigation,
            IJSRuntime js)
        {
            _http = http;
            _navigation = navigation;
            _js = js;
        }

        public async Task<bool> AddToCartAsync(Book book)
        {
            try
            {
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
                    return false;
                }

                var apiResponse = await httpResponse.Content
                    .ReadFromJsonAsync<ApiResponse<GetCartItem?>>();

                if (httpResponse.IsSuccessStatusCode &&
                    apiResponse?.WasSuccessful == true)
                {
                    return true;
                }

                await _js.InvokeVoidAsync(
                    "alert",
                    apiResponse?.Message ?? "Failed to add item to cart."
                );
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddToCart error: {ex.Message}");
                await _js.InvokeVoidAsync("alert", "Unexpected error occurred.");
                return false;
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

                if (httpResponse.IsSuccessStatusCode &&
                    response?.WasSuccessful == true)
                {
                    return (response.Data?.ToList() ?? new List<GetCartItem>(), null);
                }

                return (new List<GetCartItem>(), response?.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cart fetch error: {ex.Message}");
                return (new List<GetCartItem>(), "Connection error.");
            }
        }

        public async Task<(GetCartItem? UpdatedItem, string Message, bool Success)> UpdateItemQuantityAsync(
            int cartItemId,
            int newQuantity)
        {
            if (newQuantity < 1)
                return (null, "Quantity cannot be less than 1.", false);

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

                var apiResponse = await httpResponse.Content
                    .ReadFromJsonAsync<ApiResponse<GetCartItem>>();

                if (httpResponse.IsSuccessStatusCode &&
                    apiResponse?.WasSuccessful == true)
                {
                    // Changed message to empty string on success to stop banner text contamination
                    return (apiResponse.Data, "", true);
                }

                return (null, apiResponse?.Message ?? "Server error", false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update error: {ex.Message}");
                return (null, "Unexpected error", false);
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
                    RequestUri = new Uri(_http.BaseAddress + (_http.BaseAddress?.ToString().EndsWith("/") == true ? "" : "/") + "users/cart"),
                    Content = JsonContent.Create(payload)
                };

                var httpResponse = await _http.SendAsync(request);

                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized ||
                    httpResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    _navigation.NavigateTo("login");
                    return (false, "");
                }

                var apiResponse = await httpResponse.Content
                    .ReadFromJsonAsync<ApiResponse<int?>>();

                if (httpResponse.IsSuccessStatusCode &&
                    apiResponse?.WasSuccessful == true)
                {
                    return (true, "");
                }

                return (false, apiResponse?.Message ?? "Failed to remove item");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Remove error: {ex.Message}");
                return (false, "Unexpected error");
            }
        }

        public void ProceedToCheckout()
        {
            _navigation.NavigateTo("checkout");
        }
    }
}