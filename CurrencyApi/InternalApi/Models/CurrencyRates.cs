using System.Text.Json.Serialization;

namespace InternalApi.Models;

/// <summary>
/// Курс валюты.
/// </summary>
public sealed class CurrencyRates
{
    /// <summary>
    /// Код валюты (например, "RUB").
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    /// <summary>
    /// Значение курса валюты.
    /// </summary>
    [JsonPropertyName("value")]
    public decimal Value { get; set; }
}