namespace TGBooksFrontend.Models
{
    using System.Text.Json.Serialization;
    using System.Collections.Generic;

    public class Book
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("authors")]
        public List<Author> Authors { get; set; } = new List<Author>();

        [JsonPropertyName("language")]
        public int Language { get; set; }

        [JsonPropertyName("onSale")]
        public bool OnSale { get; set; }

        [JsonPropertyName("offPercentage")]
        public int OffPercentage { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("imageURL")]
        public string ImageURL { get; set; } = string.Empty;

        [JsonPropertyName("alreadyInCart")]
        public bool AlreadyInCart { get; set; }
    }
}