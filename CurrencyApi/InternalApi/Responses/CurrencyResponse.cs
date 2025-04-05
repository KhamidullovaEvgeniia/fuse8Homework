using System.Text.Json.Serialization;

namespace InternalApi.Responses;

public class Meta
{
    [JsonPropertyName("last_updated_at")]
    public required DateTime LastUpdatedAt { get; set; }
}

public class CurrencyInfo
{
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonPropertyName("value")]
    public decimal Value { get; set; }
}

public class CurrencyResponse
{
    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }

    [JsonPropertyName("data")]
    public required Dictionary<string, CurrencyInfo> Data { get; set; }
}