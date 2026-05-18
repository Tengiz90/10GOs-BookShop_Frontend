namespace TGBooksFrontend.Models
{
    public class ApiResponse<T>
    {
        public bool WasSuccessful { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
