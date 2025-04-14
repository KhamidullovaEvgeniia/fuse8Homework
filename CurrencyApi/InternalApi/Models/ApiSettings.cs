using System.Text.Json.Serialization;

namespace InternalApi.Models;

/// <summary>
/// Настройки API.
/// </summary>
public class ApiSettings
{
    /// <summary>
    /// Валюта по умолчанию.
    /// </summary>
    [JsonPropertyName("defaultCurrency")]
    public required string DefaultCurrency { get; set; }

    /// <summary>
    /// Базовая валюта.
    /// </summary>
    [JsonPropertyName("baseCurrency")]
    public required string BaseCurrency { get; set; }

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
}