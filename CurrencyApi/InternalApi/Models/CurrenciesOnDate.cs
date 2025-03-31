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
    [JsonPropertyName("lastUpdatedAt")]
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Список курсов валют
    /// </summary>
    [JsonPropertyName("currencies")]
    public required CurrencyRates[] Rates { get; init; }
}