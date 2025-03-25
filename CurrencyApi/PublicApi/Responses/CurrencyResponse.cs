using System.Text.Json.Serialization;

namespace Fuse8.BackendInternship.PublicApi.Responses;

public class Meta
{
    [JsonPropertyName("last_updated_at")]
    public DateTime LastUpdatedAt { get; set; }
}

public class CurrencyInfo
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public decimal Value { get; set; }
}

public class CurrencyResponse
{
    [JsonPropertyName("meta")]
    public Meta Meta { get; set; } = new();

    [JsonPropertyName("data")]
    public Dictionary<string, CurrencyInfo> Data { get; set; } = new();
}