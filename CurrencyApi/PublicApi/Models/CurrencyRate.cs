using System.Text.Json.Serialization;

namespace Fuse8.BackendInternship.PublicApi.Models;

/// <summary>
/// Курс валюты.
/// </summary>
public class CurrencyRate
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