using System.Text.Json.Serialization;

namespace Fuse8.BackendInternship.PublicApi.Settings;

public sealed class CurrencySetting
{
    public const string SectionName = "CurrencySetting";

    [JsonInclude]
    public required string BaseCurrency { get; init; }

    [JsonInclude]
    public required string Currency { get; init; }

    [JsonInclude]
    public int Accuracy { get; set; }
}