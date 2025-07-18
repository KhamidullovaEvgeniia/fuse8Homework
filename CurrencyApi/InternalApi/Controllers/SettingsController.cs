﻿using InternalApi.Interfaces;
using InternalApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace InternalApi.Controllers;

/// <summary>
/// Метод для получения текущих настроек API.
/// </summary>
[Route("settings")]
public class SettingsController : ControllerBase
{
    private readonly ICurrencyAPI _currencyApiService;

    /// <summary>
    /// Конструктор контроллера.
    /// </summary>
    /// <param name="currencyApiService">Сервис для работы с API валют.</param>
    public SettingsController(ICurrencyAPI currencyApiService)
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
    public async Task<ApiSettings> GetSettingsAsync(CancellationToken cancellationToken)
    {
        return await _currencyApiService.GetApiSettingsAsync(cancellationToken);
    }
}