using Fuse8.BackendInternship.PublicApi.Interfaces;
using Fuse8.BackendInternship.PublicApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fuse8.BackendInternship.PublicApi.Controllers;

/// <summary>
/// Методы для операций над Избранными курсами валют
/// </summary>
/// 
[Route("favoriteCurrency")]
public class FavoriteCurrencyController : ControllerBase
{
    private readonly IFavoriteCurrencyService _favoriteCurrencyService;

    /// <summary>
    /// Конструктор контроллера.
    /// </summary>
    /// <param name="favoriteCurrencyService">Сервис для получения данных об Избранных курсах валют.</param>
    public FavoriteCurrencyController(IFavoriteCurrencyService favoriteCurrencyService)
    {
        _favoriteCurrencyService = favoriteCurrencyService;
    }

    /// <summary>
    /// Получает Избранное по его названию.
    /// </summary>
    /// <param name="name">Избранное (например, "RubToUsd").</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Избранное по его названию.</returns>
    /// <response code="200">
    /// Успешный запрос, возвращает Избранное по его названию.
    /// </response>
    /// <response code="429">
    /// Превышен лимит запросов.
    /// </response>
    /// <response code="500">
    /// Ошибка сервера.
    /// </response>
    [HttpGet("by-name/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<FavoriteCurrencyRateDTO> GetFavoriteCurrencyRateByNameAsync(
        [FromRoute] string name,
        CancellationToken cancellationToken)
    {
        return await _favoriteCurrencyService.GetFavoriteCurrencyRateByNameAsync(name, cancellationToken);
    }

    /// <summary>
    /// Получает список всех Избранных.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список всех Избранных.</returns>
    /// <response code="200">
    /// Успешный запрос, возвращает список всех Избранных.
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
    public async Task<FavoriteCurrencyRateDTO[]> GetAllFavoriteCurrencyRatesAsync(CancellationToken cancellationToken)
    {
        return await _favoriteCurrencyService.GetAllFavoriteCurrencyRatesAsync(cancellationToken);
    }

    /// <summary>
    /// Получает текущий курс для Избранной валюты по её названию.
    /// </summary>
    /// <param name="name">Избранное (например, "RubToUsd").</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Текущий курс для Избранной валюты по её названию.</returns>
    /// <response code="200">
    /// Успешный запрос, возвращает текущий курс для Избранной валюты по её названию.
    /// </response>
    /// <response code="429">
    /// Превышен лимит запросов.
    /// </response>
    /// <response code="500">
    /// Ошибка сервера.
    /// </response>
    [HttpGet("by-favoriteName/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<CurrencyRate> GetSelectedCurrencyRateByName([FromRoute] string name, CancellationToken cancellationToken)
    {
        return await _favoriteCurrencyService.GetSelectedCurrencyRateByName(name, cancellationToken);
    }

    /// <summary>
    /// Получает курс для Избранного по его названию на конкретную дату.
    /// </summary>
    /// <param name="name">Избранное (например, "RubToUsd").</param>
    /// <param name="date">Дата формата yyyy-MM-dd, на которую требуется курс.</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Курс для Избранного по его названию на конкретную дату.</returns>
    /// <response code="200">
    /// Успешный запрос, возвращает курс для Избранного по его названию на конкретную дату.
    /// </response>
    /// <response code="429">
    /// Превышен лимит запросов.
    /// </response>
    /// <response code="500">
    /// Ошибка сервера.
    /// </response>
    [HttpGet("{name}/{date:datetime}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<DatedCurrencyRate> GetSelectedCurrencyRateByDate(
        [FromRoute] string name,
        [FromRoute] DateOnly date,
        CancellationToken cancellationToken)
    {
        return await _favoriteCurrencyService.GetSelectedCurrencyRateByDate(name, date, cancellationToken);
    }

    /// <summary>
    /// Добавляет новое Избранное.
    /// </summary>
    /// <param name="currencyRate">Избранное (например, "RubToUsd"), валюта, для которой нужно получить курс ("RUB"), базовая валюта ("USD").</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">
    /// Успешный запрос, добавляет новое Избранное.
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
    [HttpPost("add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task AddFavoriteCurrencyRateAsync(
        [FromBody] FavoriteCurrencyRateDTO currencyRate,
        CancellationToken cancellationToken)
    {
        await _favoriteCurrencyService.AddFavoriteCurrencyRateAsync(currencyRate, cancellationToken);
    }

    /// <summary>
    /// Изменяет Избранное по его названию.
    /// </summary>
    /// <param name="currencyRate">Избранное (например, "RubToUsd"), валюта, для которой нужно получить курс ("RUB"), базовая валюта ("USD").</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <response code="200">
    /// Успешный запрос, изменяет Избранное по его названию.
    /// </response>
    /// /// <response code="404">
    /// Курс валюты не найден.
    /// </response>
    /// <response code="429">
    /// Превышен лимит запросов.
    /// </response>
    /// <response code="500">
    /// Ошибка сервера.
    /// </response>
    [HttpPut("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task UpdateFavoriteCurrencyRateAsync(
        [FromBody] FavoriteCurrencyRateDTO currencyRate,
        CancellationToken cancellationToken)
    {
        await _favoriteCurrencyService.UpdateFavoriteCurrencyRateAsync(currencyRate, cancellationToken);
    }

    /// <summary>
    /// Удаляет Избранное по его названию.
    /// </summary>
    /// <param name="name">Избранное (например, "RubToUsd").</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <response code="200">
    /// Успешный запрос, удаляет Избранное по его названию.
    /// </response>
    /// <response code="429">
    /// Превышен лимит запросов.
    /// </response>
    /// <response code="500">
    /// Ошибка сервера.
    /// </response>
    [HttpDelete("delete/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task DeleteFavoriteCurrencyRateByNameAsync([FromRoute] string name, CancellationToken cancellationToken)
    {
        await _favoriteCurrencyService.DeleteFavoriteCurrencyRateByNameAsync(name, cancellationToken);
    }
}