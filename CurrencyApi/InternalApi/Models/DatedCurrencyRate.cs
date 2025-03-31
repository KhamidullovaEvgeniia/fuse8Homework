using System.Text.Json.Serialization;

namespace InternalApi.Models;

/// <summary>
/// Курс валюты с датой.
/// </summary>
public class DatedCurrencyRate
{
    /// <summary>
    /// Дата курса валюты в формате yyyy-MM-dd.
    /// </summary>

    [JsonPropertyName("date")]
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Код валюты (например, "RUB").
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    /// <summary>
    /// Значение курса валюты.
    /// </summary>
    [JsonPropertyName("value")]
    public decimal Value { get; set; }
}