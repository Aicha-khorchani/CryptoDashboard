using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using CryptoDashboard.Models;
using CryptoDashboard.Helpers;
using System.Threading;


namespace CryptoDashboard.Services
{
    public class CoinGeckoService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.coingecko.com/api/v3/")
        };
        private static bool _rateLimitToastShown = false;

        static CoinGeckoService()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CryptoDashboardApp/1.0");
        }
        public async Task<List<Coin>> GetMarketCoinsAsync(string vsCurrency = "usd")
        {
            var url =
                $"coins/markets?vs_currency={vsCurrency}" +
                "&order=market_cap_desc" +
                "&per_page=50&page=1" +
                "&sparkline=false";
            Logger.Log("REQUEST → " + url);
            var response = await _httpClient.GetAsync(url);
            Logger.Log("STATUS → " + response.StatusCode);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            Logger.Log("JSON LENGTH → " + json.Length);
            Logger.Log("First coin JSON → " + (json.Length > 1000 ? json.Substring(0, 1000) : json));
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var coins = JsonSerializer.Deserialize<List<Coin>>(json, options);
            Logger.Log("Coins returned: " + (coins?.Count ?? 0));

            return coins ?? new List<Coin>();
        }

        public async Task<List<PricePoint>> GetMarketChartAsync(
            string coinId,
            int days,
            string vsCurrency = "usd",
            CancellationToken token = default)
        {
            var url = $"coins/{coinId}/market_chart?vs_currency={vsCurrency}&days={days}";

            var response = await _httpClient.GetAsync(url, token);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                Logger.Log("RATE LIMITED chart for " + coinId);
                if (!_rateLimitToastShown)
                {
                    _rateLimitToastShown = true;

                    // Show toast asynchronously (don’t await to not block)
                    _ = ToastHelper.ShowToastAsync("Rate limit reached, please wait...");

                    // Reset flag after 5 seconds so user can see it again if still rate-limited
                    Task.Delay(5000).ContinueWith(_ => _rateLimitToastShown = false);
                }
                return new List<PricePoint>();
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(token);

            using var doc = JsonDocument.Parse(json);

            var prices = doc.RootElement.GetProperty("prices");
            var volumes = doc.RootElement.GetProperty("total_volumes");

            var result = new List<PricePoint>();

            int count = Math.Min(prices.GetArrayLength(), volumes.GetArrayLength());

            for (int i = 0; i < count; i++)
            {
                var priceItem = prices[i];
                var volumeItem = volumes[i];

                var timestamp = priceItem[0].GetInt64();
                var price = priceItem[1].GetDecimal();
                var volume = volumeItem[1].GetDecimal();

                result.Add(new PricePoint
                {
                    Date = DateTimeOffset
                        .FromUnixTimeMilliseconds(timestamp)
                        .DateTime,
                    Price = price,
                    Volume = volume
                });
            }
            return result;
        }
    }
}
