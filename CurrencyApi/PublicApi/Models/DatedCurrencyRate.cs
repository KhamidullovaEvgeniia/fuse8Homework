using System.Text.Json.Serialization;

namespace Fuse8.BackendInternship.PublicApi.Models;

/// <summary>
/// Курс валюты с датой.
/// </summary>
public class DatedCurrencyRate
{
    /// <summary>
    /// Дата курса валюты в формате yyyy-MM-dd.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("date")]
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Код валюты (например, "RUB").
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    /// <summary>
    /// Значение курса валюты.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("value")]
    public decimal Value { get; set; }
}