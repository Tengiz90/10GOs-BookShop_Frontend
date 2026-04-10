using System.Text.Json.Serialization;

namespace TGBooksFrontend.Models
{
    public class Author
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
