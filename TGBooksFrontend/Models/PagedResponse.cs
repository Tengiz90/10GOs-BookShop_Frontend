using System.Text.Json.Serialization;

namespace TGBooksFrontend.Models;

public class PagedResponse
{
    [JsonPropertyName("data")]
    public List<Book> Data { get; set; } = new();
}