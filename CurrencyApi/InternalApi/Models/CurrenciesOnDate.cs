using System.Text.Json.Serialization;

namespace InternalApi.Models;
/// <summary>
/// Курс валюты с датой.
/// </summary>
public class CurrenciesOnDate
{
    /// <summary>
    /// Дата обновления данных
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("lastUpdatedAt")]
    public required DateTime Date { get; init; }

    /// <summary>
    /// Список курсов валют
    /// </summary>
    [JsonPropertyName("currencies")]
    public required CurrencyRates[] Rates { get; init; }
}