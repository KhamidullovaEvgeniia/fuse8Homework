using System.Text.Json.Serialization;

namespace InternalApi.Responses;

public class CurrencyInfo
{
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonPropertyName("value")]
    public decimal Value { get; set; }
}

public class CurrencyResponse
{
    [JsonPropertyName("data")]
    public required Dictionary<string, CurrencyInfo> Data { get; set; }
}