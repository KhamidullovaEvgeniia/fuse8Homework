using System.Text.Json.Serialization;

namespace InternalApi.Settings;

public sealed class CurrencySetting
{
    public const string SectionName = "CurrencySetting";

    [JsonInclude]
    public required TimeSpan CacheExpiration { get; init; }
}