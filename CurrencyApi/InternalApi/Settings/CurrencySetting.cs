namespace InternalApi.Settings;

public class CurrencySetting
{
    public required string BaseCurrency { get; init; }
    public string Currency { get; init; } 
    public int Accuracy { get; set; }
}