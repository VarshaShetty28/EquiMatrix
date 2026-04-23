using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BackEnd.Services
{
    public class MarketPriceService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _cacheFilePath;
        private readonly string _priceLogFilePath;
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public MarketPriceService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _cacheFilePath = Path.Combine(Directory.GetCurrentDirectory(), "market_price_cache.json");
            _priceLogFilePath = Path.Combine(Directory.GetCurrentDirectory(), "market_price_log.json");
        }

        private record PriceLogEntry(DateTime Timestamp, decimal Price);

        /* ======================================================
           CACHE HELPERS
        ====================================================== */

        private async Task<Dictionary<string, JsonElement>> LoadCacheAsync()
        {
            if (!File.Exists(_cacheFilePath))
                return new Dictionary<string, JsonElement>();

            var json = await File.ReadAllTextAsync(_cacheFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)
                   ?? new Dictionary<string, JsonElement>();
        }

        private async Task SaveToCacheAsync(string symbol, JsonElement timeSeriesDaily)
        {
            var cache = await LoadCacheAsync();
            cache[symbol] = timeSeriesDaily;

            var json = JsonSerializer.Serialize(cache, _jsonOptions);
            await File.WriteAllTextAsync(_cacheFilePath, json);
        }

        private async Task<Dictionary<string, List<PriceLogEntry>>> LoadPriceLogAsync()
        {
            if (!File.Exists(_priceLogFilePath))
                return new Dictionary<string, List<PriceLogEntry>>();

            var json = await File.ReadAllTextAsync(_priceLogFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, List<PriceLogEntry>>>(json)
                   ?? new Dictionary<string, List<PriceLogEntry>>();
        }

        private async Task SavePriceLogEntryAsync(string symbol, decimal price)
        {
            var log = await LoadPriceLogAsync();
            if (!log.TryGetValue(symbol, out var entries))
            {
                entries = new List<PriceLogEntry>();
                log[symbol] = entries;
            }

            entries.Add(new PriceLogEntry(DateTime.UtcNow, price));
            var json = JsonSerializer.Serialize(log, _jsonOptions);
            await File.WriteAllTextAsync(_priceLogFilePath, json);
        }

        private async Task<decimal?> GetLatestLoggedPriceAsync(string symbol)
        {
            var log = await LoadPriceLogAsync();
            if (!log.TryGetValue(symbol, out var entries) || entries.Count == 0)
                return null;

            return entries.OrderByDescending(e => e.Timestamp).First().Price;
        }

        private async Task<decimal?> GetLatestPriceFromCacheAsync(string symbol, DateTime? date = null)
        {
            var cache = await LoadCacheAsync();
            if (cache.TryGetValue(symbol, out var cachedSeries))
            {
                return ParseAdjustedClose(cachedSeries, date);
            }
            return null;
        }

        private async Task<decimal?> GetFallbackPriceAsync(string symbol, DateTime? date = null)
        {
            var loggedPrice = await GetLatestLoggedPriceAsync(symbol);
            if (loggedPrice.HasValue)
                return loggedPrice;

            return await GetLatestPriceFromCacheAsync(symbol, date);
        }

        /* ======================================================
           PARSING HELPERS
        ====================================================== */

        private decimal? ParseAdjustedClose(JsonElement timeSeries, DateTime? date)
        {
            if (date.HasValue)
            {
                var dateKey = date.Value.ToString("yyyy-MM-dd");
                if (timeSeries.TryGetProperty(dateKey, out var dayData) &&
                    dayData.TryGetProperty("4. close", out var close))
                {
                    return decimal.Parse(close.GetString());
                }
                return null;
            }

            var latestDay = timeSeries.EnumerateObject()
                .OrderByDescending(e => e.Name)
                .FirstOrDefault();

            if (latestDay.Value.ValueKind != JsonValueKind.Object)
                return null;

            return latestDay.Value.TryGetProperty("4. close", out var latestClose)
                ? decimal.Parse(latestClose.GetString())
                : null;
        }

        private Dictionary<string, decimal> ParseHistory(JsonElement timeSeries, int days)
        {
            var historyEntries = timeSeries.EnumerateObject()
                .Select(entry =>
                {
                    if (DateTime.TryParse(entry.Name, out var date)
                        && entry.Value.TryGetProperty("4. close", out var close))
                    {
                        return (Date: date, Key: entry.Name, Price: decimal.Parse(close.GetString()));
                    }

                    return (Date: DateTime.MinValue, Key: string.Empty, Price: 0m);
                })
                .Where(item => item.Date != DateTime.MinValue)
                .OrderByDescending(item => item.Date)
                .Take(days)
                .ToList();

            return historyEntries
                .ToDictionary(item => item.Key, item => item.Price);
        }

        /* ======================================================
           PUBLIC API METHODS
        ====================================================== */

        public async Task<decimal?> GetAdjustedClosePriceAsync(string symbol, DateTime? date = null)
        {
            var apiKey = _configuration["AlphaVantage:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("AlphaVantage API key is not configured. Set AlphaVantage:ApiKey in appsettings.json or environment variables.");
            }

            var url =
                $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={apiKey}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);

                if (doc.RootElement.TryGetProperty("Note", out _)
                    || doc.RootElement.TryGetProperty("Error Message", out _)
                    || doc.RootElement.TryGetProperty("Information", out _)
                    || !doc.RootElement.TryGetProperty("Time Series (Daily)", out var timeSeries))
                {
                    return await GetFallbackPriceAsync(symbol, date);
                }

                await SaveToCacheAsync(symbol, timeSeries);

                var latestPrice = ParseAdjustedClose(timeSeries, date);
                if (latestPrice.HasValue)
                {
                    await SavePriceLogEntryAsync(symbol, latestPrice.Value);
                }

                return latestPrice;
            }
            catch (Exception)
            {
                return await GetFallbackPriceAsync(symbol, date);
            }
        }

        public async Task<Dictionary<string, decimal>> GetPriceHistoryAsync(string symbol, int days = 30)
        {
            var apiKey = _configuration["AlphaVantage:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("AlphaVantage API key is not configured. Set AlphaVantage:ApiKey in appsettings.json or environment variables.");
            }

            var url =
                $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={apiKey}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);

                if (doc.RootElement.TryGetProperty("Note", out _)
                    || doc.RootElement.TryGetProperty("Error Message", out _)
                    || doc.RootElement.TryGetProperty("Information", out _)
                    || !doc.RootElement.TryGetProperty("Time Series (Daily)", out var timeSeries))
                {
                    var cache = await LoadCacheAsync();
                    if (cache.TryGetValue(symbol, out var cachedSeries))
                    {
                        return ParseHistory(cachedSeries, days);
                    }
                    return new Dictionary<string, decimal>();
                }

                await SaveToCacheAsync(symbol, timeSeries);

                var latestPrice = ParseAdjustedClose(timeSeries, null);
                if (latestPrice.HasValue)
                {
                    await SavePriceLogEntryAsync(symbol, latestPrice.Value);
                }

                return ParseHistory(timeSeries, days);
            }
            catch (Exception)
            {
                var cache = await LoadCacheAsync();
                if (cache.TryGetValue(symbol, out var cachedSeries))
                {
                    return ParseHistory(cachedSeries, days);
                }
                return new Dictionary<string, decimal>();
            }
        }
    }
}