using System.Text.Json.Serialization;

namespace Fuse8.BackendInternship.PublicApi.Models;

/// <summary>
/// Настройки API.
/// </summary>
public class ApiSettings
{
    /// <summary>
    /// Валюта по умолчанию.
    /// </summary>
    [JsonPropertyName("defaultCurrency")]
    public string DefaultCurrency { get; set; }

    /// <summary>
    /// Базовая валюта.
    /// </summary>
    [JsonPropertyName("baseCurrency")]
    public string BaseCurrency { get; set; }

    /// <summary>
    /// Лимит запросов к API.
    /// </summary>
    [JsonPropertyName("requestLimit")]
    public int RequestLimit { get; set; }

    /// <summary>
    /// Количество уже выполненных запросов.
    /// </summary>
    [JsonPropertyName("requestCount")]
    public int RequestCount { get; set; }

    /// <summary>
    /// Количество знаков после запятой для округления валюты.
    /// </summary>
    [JsonPropertyName("currencyRoundCount")]
    public int CurrencyRoundCount { get; set; }
}