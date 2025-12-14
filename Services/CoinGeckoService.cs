using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using CryptoDashboard.Models;

namespace CryptoDashboard.Services
{
    public class CoinGeckoService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.coingecko.com/api/v3/")
        };

        public async Task<List<Coin>> GetMarketCoinsAsync(string vsCurrency = "usd")
        {
            var url =
                $"coins/markets?vs_currency={vsCurrency}" +
                "&order=market_cap_desc" +
                "&per_page=50&page=1" +
                "&sparkline=false";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();


            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var coins = JsonSerializer.Deserialize<List<Coin>>(json, options);
Console.WriteLine("Coins returned: " + (coins?.Count ?? 0));

            return coins ?? new List<Coin>();
        }

        public async Task<List<PricePoint>> GetMarketChartAsync(
            string coinId,
            int days,
            string vsCurrency = "usd")
        {
            var url = $"coins/{coinId}/market_chart?vs_currency={vsCurrency}&days={days}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            var prices = doc.RootElement.GetProperty("prices");

            var result = new List<PricePoint>();

            foreach (var item in prices.EnumerateArray())
            {
                var timestamp = item[0].GetInt64();
                var price = item[1].GetDecimal();

                result.Add(new PricePoint
                {
                    Date = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime,
                    Price = price
                });
            }

            return result;
        }
    }
}
