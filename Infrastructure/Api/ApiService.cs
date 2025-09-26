using System.Net.Http.Json;
using System.Text.Json;
using MoneyTracker.Infrastructure.Api.Models;

namespace MoneyTracker.Infrastructure.Api;

/// <summary>
/// Service used for integrations with external APIs
/// (exchange rates, cloud synchronization, etc.).
/// </summary>
public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Retrieves current exchange rates (sample implementation).
    /// </summary>
    public async Task<ExchangeRatesResponse?> GetExchangeRatesAsync(string baseCurrency = "USD")
    {
        try
        {
            // Example using a free exchange-rate API
            var response = await _httpClient.GetAsync($"https://api.exchangerate-api.com/v4/latest/{baseCurrency}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ExchangeRatesResponse>(jsonString, _jsonOptions);
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting exchange rates: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Synchronizes data with the server (placeholder for future work).
    /// </summary>
    public async Task<bool> SyncDataAsync<T>(T data) where T : class
    {
        try
        {
            // Logic to synchronize with your backend would go here
            var response = await _httpClient.PostAsJsonAsync("/api/sync", data, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error syncing data: {ex.Message}");
            return false;
        }
    }
}