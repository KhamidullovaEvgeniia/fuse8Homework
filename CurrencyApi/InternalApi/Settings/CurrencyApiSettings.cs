using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InternalApi.Settings;

public sealed class CurrencyApiSettings
{
    public const string SectionName = "CurrencyApiSettings";

    [JsonInclude]
    [Required(AllowEmptyStrings = false)]
    public required string BaseUrl { get; init; }

    [JsonInclude]
    [Required(AllowEmptyStrings = false)]
    public required string ApiKey { get; init; }
}