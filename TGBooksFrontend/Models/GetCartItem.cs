using System.Text.Json.Serialization;

namespace TGBooksFrontend.Models
{
    public class GetCartItem
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Language { get; set; } // Treated as an integer representation
        public int Quantity { get; set; }
        public bool OnSale { get; set; }
        public decimal OriginalPrice { get; set; }
        public int OffPercentage { get; set; }
        public string ImageURL { get; set; } = string.Empty;
    }
}
