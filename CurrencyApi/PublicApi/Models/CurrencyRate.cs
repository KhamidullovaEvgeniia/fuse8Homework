namespace Fuse8.BackendInternship.PublicApi.Models;

/// <summary>
/// Курс валюты.
/// </summary>
public class CurrencyRate
{
    /// <summary>
    /// Код валюты (например, "RUB").
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Значение курса валюты.
    /// </summary>
    public decimal Value { get; set; }
}