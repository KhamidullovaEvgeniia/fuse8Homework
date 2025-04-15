using System.Text.Json.Serialization;

namespace InternalApi.Responses;

public class Quotas
{
    [JsonPropertyName("month")]
    public QuotaDetails Month { get; set; } = new();
}

public class QuotaDetails
{
    [JsonInclude]
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonInclude]
    [JsonPropertyName("used")]
    public int Used { get; set; }
}

public class QuotaResponse
{
    [JsonPropertyName("quotas")]
    public Quotas Quotas { get; set; } = new();
}