using System.Text.Json.Serialization;

namespace InternalApi.Models;

/// <summary>
/// Настройки API.
/// </summary>
public sealed class ApiSettings
{
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