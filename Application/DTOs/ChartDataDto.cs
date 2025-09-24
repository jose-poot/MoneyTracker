namespace MoneyTracker.Application.DTOs;

public class ChartDataDto
{
    public string Label { get; set; } = string.Empty;
    public float Value { get; set; }
    public string Color { get; set; } = "#2196F3";
}