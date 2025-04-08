using System.Text.Json.Serialization;

namespace Fuse8.BackendInternship.PublicApi.Responses;

public class Quotas
{
    [JsonPropertyName("month")]
    public QuotaDetails Month { get; set; } = new();
}

public class QuotaDetails
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("used")]
    public int Used { get; set; }
}

public class QuotaResponse
{
    [JsonPropertyName("quotas")]
    public Quotas Quotas { get; set; } = new();
}