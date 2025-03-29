using System.Text.Json.Serialization;

namespace Fuse8.BackendInternship.PublicApi.Responses;
// TODO: убрать лишнее
public class Quotas
{
    [JsonPropertyName("month")]
    public QuotaDetails Month { get; set; } = new();

    [JsonPropertyName("grace")]
    public QuotaDetails Grace { get; set; } = new();
}

public class QuotaDetails
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("used")]
    public int Used { get; set; }

    [JsonPropertyName("remaining")]
    public int Remaining { get; set; }
}

public class QuotaResponse
{
    [JsonPropertyName("account_id")]
    public long AccountId { get; set; }

    [JsonPropertyName("quotas")]
    public Quotas Quotas { get; set; } = new();
}