using Fuse8.BackendInternship.InternalApi.Contracts;
using InternalApi.Enums;
using InternalApi.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using InternalApi.Models;
using InternalApi.Settings;

namespace InternalApi.Controllers;

/// <summary>
/// Методы для получения актуального курс валюты, курса валюты по коду и курс на определенную дату.
/// </summary>
[Route("currency")]
public class CurrencyController : ControllerBase
{
    private readonly ICachedCurrencyAPI _cachedCurrencyAPI;

    private readonly int _currencyCode;

    /// <summary>
    /// Конструктор контроллера.
    /// </summary>
    /// <param name="currencyApiService">Сервис для получения данных о валюте.</param>
    /// <param name="currencySetting">Настройки валюты, содержащие валюту по умолчанию, дефолтную валюту и количество знаков после запятой.</param>
    public CurrencyController(
        ICachedCurrencyAPI cachedCurrencyApi,
        IOptions<CurrencySetting> currencySetting)
    {
        _cachedCurrencyAPI = cachedCurrencyApi;
        _currencyCode = currencySetting.Value.Currency;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<CurrencyDTO> GetCurrencyRateAsync(CancellationToken cancellationToken)
    {
        return await _cachedCurrencyAPI.GetCurrentCurrencyAsync((CurrencyType)_currencyCode, cancellationToken);
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<CurrencyDTO> GetDatedCurrencyRateAsync(
        [FromRoute] int currencyCode,
        [FromRoute] DateOnly date,
        CancellationToken cancellationToken)
    {
        return await _cachedCurrencyAPI.GetCurrencyOnDateAsync((CurrencyType)currencyCode, date, cancellationToken);
    }
}