namespace TGBooksFrontend.Models
{
    using System.Text.Json.Serialization;

    public class Book
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("authors")]
        public List<Author> Authors { get; set; }

        [JsonPropertyName("language")]
        public int Language { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("imageURL")]
        public string ImageURL { get; set; }

        [JsonPropertyName("alreadyInCart")]
        public bool AlreadyInCart { get; set; }
    }
}
