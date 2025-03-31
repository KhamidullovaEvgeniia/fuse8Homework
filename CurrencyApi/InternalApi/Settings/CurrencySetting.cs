namespace InternalApi.Settings;

public class CurrencySetting
{
    public required string BaseCurrency { get; init; }
    public int Currency { get; init; } 
    public int Accuracy { get; set; }
}