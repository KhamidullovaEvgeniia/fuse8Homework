using Fuse8.BackendInternship.PublicApi.Interfaces;
using Fuse8.BackendInternship.PublicApi.Models;
using Fuse8.BackendInternship.PublicApi.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Fuse8.BackendInternship.PublicApi.Controllers;
/// <summary>
/// Метод для получения текущих настроек API.
/// </summary>
[Route("settings")]
public class SettingsController : ControllerBase
{
    private readonly ICurrencyApiService _currencyApiService;
    /// <summary>
    /// Конструктор контроллера.
    /// </summary>
    /// <param name="currencyApiService">Сервис для работы с API валют.</param>
    public SettingsController(ICurrencyApiService currencyApiService)
    {
        _currencyApiService = currencyApiService;
    }

    /// <summary>
    /// Получает текущие настройки API.
    /// </summary>
    /// <returns>Настройки API.</returns>
    /// <response code="200">Успешный запрос, возвращает настройки API.</response>
    /// <response code="429">Превышен лимит запросов.</response>
    /// <response code="500">Ошибка сервера.</response>
    [HttpGet]
    public async Task<ApiSettings> GetSettingsAsync()
    {
        return await _currencyApiService.GetApiSettingsAsync();
    }
}