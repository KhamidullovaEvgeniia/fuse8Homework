using System.Text.Json.Serialization;

namespace InternalApi.Responses;

public class Meta
{
    [JsonInclude] 
    [JsonPropertyName("last_updated_at")]
    public required DateTime LastUpdatedAt { get; set; }
}

public class CurrencyInfo
{
    [JsonInclude] 
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonInclude] 
    [JsonPropertyName("value")]
    public decimal Value { get; set; }
}

public class CurrencyResponse
{
    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }

    [JsonPropertyName("data")]
    public required Dictionary<string, CurrencyInfo> Data { get; init; }
}