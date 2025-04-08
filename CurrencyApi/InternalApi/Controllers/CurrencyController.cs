using Framework.Enums;
using Framework.Helper;
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

    private readonly string _currencyCode;

    /// <summary>
    /// Конструктор контроллера.
    /// </summary>
    /// <param name="cachedCurrencyApi">Сервис для получения данных о валюте</param>
    /// <param name="currencySetting">Настройки валюты, содержащие валюту по умолчанию, дефолтную валюту и количество знаков после запятой.</param>
    public CurrencyController(ICachedCurrencyAPI cachedCurrencyApi, IOptions<CurrencySetting> currencySetting)
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
    public async Task<ActionResult<CurrencyDTO>> GetCurrencyRateAsync(
        [FromQuery] CurrencyType baseCurrencyType,
        [FromQuery] CurrencyType currencyType,
        CancellationToken cancellationToken)
    {
        if (currencyType == CurrencyType.NotSet)
            return BadRequest();

        if (baseCurrencyType == CurrencyType.NotSet)
            return BadRequest();

        return await _cachedCurrencyAPI.GetCurrentCurrencyAsync(baseCurrencyType, currencyType, cancellationToken);
    }

    /// <summary>
    /// Получает курс указанной валюты на определенную дату.
    /// </summary>
    /// <param name="baseCurrencyCode">Код базовой валюты, относительно которой необходимо вычислить курс, например, "USD"</param>
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
        [FromRoute] string baseCurrencyCode,
        [FromRoute] string currencyCode,
        [FromRoute] DateOnly date,
        CancellationToken cancellationToken)
    {
        var baseCurrencyType = CurrencyHelper.ParsingCurrencyCode(baseCurrencyCode);
        var currencyType = CurrencyHelper.ParsingCurrencyCode(currencyCode);
        return await _cachedCurrencyAPI.GetCurrencyOnDateAsync(baseCurrencyType, currencyType, date, cancellationToken);
    }
}