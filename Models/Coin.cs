using System.Text.Json.Serialization;


namespace CryptoDashboard.Models
{
    public class Coin
    {
        public string Id { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal? AlertPrice { get; set; }
        public bool AlertTriggered { get; set; }

        [JsonPropertyName("current_price")]
        public decimal CurrentPrice { get; set; }

        [JsonPropertyName("market_cap")]
        public decimal MarketCap { get; set; }

        [JsonPropertyName("price_change_percentage_24h")]
        public decimal PriceChangePercentage24h { get; set; }

        [JsonPropertyName("total_volume")]
        public decimal TotalVolume { get; set; }

        [JsonPropertyName("image")]
        public string ImageUrl { get; set; } = string.Empty;
        
        [JsonIgnore]
        public bool IsFavorite { get; set; } = false;
    }
}
