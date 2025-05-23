﻿using System.ComponentModel.DataAnnotations;

namespace InternalApi.Settings;

public class CurrencyApiSettings
{
    public const string SectionName = "CurrencyApiSettings";

    [Required(AllowEmptyStrings = false)]
    public required string BaseUrl { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string ApiKey { get; init; }
}