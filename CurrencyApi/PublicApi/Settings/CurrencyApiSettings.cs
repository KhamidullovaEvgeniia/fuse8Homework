using System.Text.Json.Serialization;

namespace Fuse8.BackendInternship.PublicApi.Settings;

public class CurrencyApiSettings
{
    public const string SectionName = "CurrencyApiSettings";

    [JsonInclude]
    public required string GrpcUrl { get; init; }
}