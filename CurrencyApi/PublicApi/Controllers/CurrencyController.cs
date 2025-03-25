using Fuse8.BackendInternship.PublicApi.Interfaces;
using Fuse8.BackendInternship.PublicApi.Models;
using Fuse8.BackendInternship.PublicApi.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Fuse8.BackendInternship.PublicApi.Controllers;

/// <summary>
/// Методы для получения актуального курс валюты, курса валюты по коду и курс на определенную дату.
/// </summary>
[Route("currency")]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyApiService _currencyApiService;

    private readonly string _defaultcurrency;

    /// <summary>
    /// Конструктор контроллера.
    /// </summary>
    /// <param name="currencyApiService">Сервис для получения данных о валюте.</param>
    /// <param name="currencySetting">Настройки валюты, содержащие валюту по умолчанию, дефолтную валюту и количество знаков после запятой.</param>
    public CurrencyController(ICurrencyApiService currencyApiService, IOptions<CurrencySetting> currencySetting)
    {
        _currencyApiService = currencyApiService;

        _defaultcurrency = currencySetting.Value.DefaultCurrency;
    }

    /// <summary>
    /// Получает текущий курс валюты по умолчанию.
    /// </summary>
    /// <returns>Текущий курс валюты.</returns>
    /// <response code="200">
    /// Успешный запрос, возвращает курс валюты.
    /// </response>
    /// <response code="404">
    /// Курс валюты не найден.
    /// </response>
    /// <response code="429">
    /// Превышен лимит запросов.
    /// </response>
    /// <response code="500">
    /// Ошибка сервера.
    /// </response>
    [HttpGet]
    public async Task<CurrencyRate> GetCurrencyRateAsync()
    {
        return await _currencyApiService.GetCurrencyRateAsync(_defaultcurrency);
    }

    /// <summary>
    /// Получает текущий курс указанной валюты.
    /// </summary>
    /// <param name="defaultCurrency">Код валюты (например, "RUB").</param>
    /// <returns>Текущий курс валюты.</returns>
    /// <response code="200">
    /// Успешный запрос, возвращает курс валюты.
    /// </response>
    /// <response code="404">
    /// Курс валюты не найден.
    /// </response>
    /// <response code="429">
    /// Превышен лимит запросов.
    /// </response>
    /// <response code="500">
    /// Ошибка сервера.
    /// </response>
    [HttpGet("{defaultCurrency}")]
    public async Task<CurrencyRate> GetCurrencyCodeRateAsync([FromRoute] string defaultCurrency)
    {
        return await _currencyApiService.GetCurrencyRateAsync(defaultCurrency);
    }

    /// <summary>
    /// Получает курс указанной валюты на определенную дату.
    /// </summary>
    /// <param name="currencyCode">Код валюты (например, "RUB").</param>
    /// <param name="date">Дата формата yyyy-MM-dd, на которую требуется курс.</param>
    /// <returns>Курс валюты на указанную дату.</returns>
    /// <response code="200">
    /// Успешный запрос, возвращает курс валюты.
    /// </response>
    /// <response code="404">
    /// Курс валюты не найден.
    /// </response>
    /// <response code="429">
    /// Превышен лимит запросов.
    /// </response>
    /// <response code="500">
    /// Ошибка сервера.
    /// </response>
    [HttpGet("{currencyCode}/{date:datetime}")]
    public async Task<DatedCurrencyRate> GetDatedCurrencyRateAsync([FromRoute] string currencyCode, [FromRoute] DateTime date)
    {
        return await _currencyApiService.GetCurrencyDataWithRateAsync(currencyCode, date);
    }
}