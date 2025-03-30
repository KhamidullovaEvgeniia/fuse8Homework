using System.ComponentModel.DataAnnotations;

namespace Fuse8.BackendInternship.PublicApi.Settings;

public class CurrencyApiSettings
{
    public const string SectionName = "CurrencyApiSettings";

    public required string BaseUrl { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string ApiKey { get; init; }
}