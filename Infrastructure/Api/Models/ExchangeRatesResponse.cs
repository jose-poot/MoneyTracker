namespace MoneyTracker.Infrastructure.Api.Models;

public class ExchangeRatesResponse
{
    public string Base { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public Dictionary<string, decimal> Rates { get; set; } = new();
}