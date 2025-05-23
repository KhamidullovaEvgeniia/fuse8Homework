﻿namespace InternalApi.Settings;

public class CurrencySetting
{
    public const string SectionName = "CurrencySetting";

    public required string BaseCurrency { get; init; }

    public required string Currency { get; init; }

    public int Accuracy { get; set; }

    public required TimeSpan CacheExpiration { get; init; }
}