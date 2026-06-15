using System.Text.Json.Serialization;

namespace TGBooksFrontend.Models
{
    public class BookWithCategoriesAndDescription : Book
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("categories")]
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}
