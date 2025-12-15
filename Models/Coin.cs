using System.Text.Json.Serialization;


namespace CryptoDashboard.Models
{
    public class Coin
    {
        public string Id { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public decimal CurrentPrice { get; set; }

        public decimal MarketCap { get; set; }

        public decimal PriceChangePercentage24h { get; set; }

        public decimal TotalVolume { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
        
        [JsonIgnore]
        public bool IsFavorite { get; set; } = false;
    }
}
