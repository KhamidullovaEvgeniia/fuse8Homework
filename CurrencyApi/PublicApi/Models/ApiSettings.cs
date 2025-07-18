﻿using System.Text.Json.Serialization;

namespace Fuse8.BackendInternship.PublicApi.Models;

/// <summary>
/// Настройки API.
/// </summary>
public sealed class ApiSettings
{
    /// <summary>
    /// Валюта по умолчанию.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("defaultCurrency")]
    public required string DefaultCurrency { get; set; }

    /// <summary>
    /// Есть ли доступные запросы к API.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("newRequestsAvailable")]
    public bool NewRequestsAvailable { get; set; }

    /// <summary>
    /// Количество знаков после запятой для округления валюты.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("currencyRoundCount")]
    public int CurrencyRoundCount { get; set; }
}