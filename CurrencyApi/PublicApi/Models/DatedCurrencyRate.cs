namespace Fuse8.BackendInternship.PublicApi.Models;

/// <summary>
/// Курс валюты с датой.
/// </summary>
public class DatedCurrencyRate
{
    /// <summary>
    /// Дата курса валюты в формате yyyy-MM-dd.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Код валюты (например, "RUB").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Значение курса валюты.
    /// </summary>
    public decimal Value { get; set; }
}