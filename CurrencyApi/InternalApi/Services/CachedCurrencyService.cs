using System.Globalization;
using System.Text.Json;
using InternalApi.Enums;
using InternalApi.Filters;
using InternalApi.Interfaces;
using InternalApi.Models;
using InternalApi.Settings;
using Microsoft.Extensions.Options;

namespace InternalApi.Services;

public class CachedCurrencyService : ICachedCurrencyAPI
{
    private readonly ICurrencyAPI _currencyApi;

    private readonly CurrencySetting _currencySetting;

    private readonly ILogger<ExceptionFilter> _logger;

    public CachedCurrencyService(
        ICurrencyAPI currencyApi,
        IOptionsSnapshot<CurrencySetting> currencySetting,
        ILogger<ExceptionFilter> logger)
    {
        _currencyApi = currencyApi;
        _currencySetting = currencySetting.Value;
        Directory.CreateDirectory(_currencySetting.CacheDirectory);
        _logger = logger;
    }

    public async Task<CurrencyDTO> GetCurrentCurrencyAsync(CurrencyType currencyType, CancellationToken cancellationToken)
    {
        string? latestFile = GetLatestCacheFile();
        if (latestFile is not null)
        {
            var currencyFromCache = await GetCurrencyFromCache(latestFile, currencyType, cancellationToken);
            if (currencyFromCache is not null)
                return currencyFromCache;
        }

        var baseCurrency = _currencySetting.BaseCurrency;
        var currencies = await _currencyApi.GetAllCurrentCurrenciesAsync(baseCurrency, cancellationToken);
        
        return await GetCurrencyDto(currencies, currencyType);
    }

    public async Task<CurrencyDTO> GetCurrencyOnDateAsync(
        CurrencyType currencyType,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        string? dateFile = GetCacheFileForDate(date);
        if (dateFile is not null)
        {
            var currencyFromCache = await GetCurrencyForDateFromCache(dateFile, currencyType, cancellationToken);
            if (currencyFromCache is not null)
                return currencyFromCache;
        }

        var baseCurrency = _currencySetting.BaseCurrency;
        var currenciesOnDate = await _currencyApi.GetAllCurrenciesOnDateAsync(baseCurrency, date, cancellationToken);

        return await GetCurrencyDto(currenciesOnDate, currencyType);
    }

    private string? GetLatestCacheFile()
    {
        return Directory.GetFiles(_currencySetting.CacheDirectory).OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
    }

    private string? GetCacheFileForDate(DateOnly date)
    {
        // TODO: доработать время файла
        var datePart = date.ToString("yyyy-MM-dd");
        return Directory
            .GetFiles(_currencySetting.CacheDirectory)
            .Where(f => f.Contains(datePart))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            // .OrderByDescending(t => TimeSpan.ParseExact(t, "yyyy-MM-dd_hh'-'mm", null))
            .FirstOrDefault();
    }

    private async Task WriteCacheAsync(CurrencyDTO?[] data, DateTime date)
    {
        string fileName = date.ToString("yyyy-MM-dd_HH-mm");
        string filePath = Path.Combine(_currencySetting.CacheDirectory, $"{fileName}.json");
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
    }

    private async Task<CurrencyDTO?> GetCurrencyFromCache(
        string filePath,
        CurrencyType currencyType,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
            return null;

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        if (!TryParseCacheDate(fileName, out var fileDateTime))
        {
            _logger.LogWarning("Не удалось распарсить дату из имени кэш-файла: {FileName}", fileName);
            return null;
        }

        // Проверка свежести по дате из имени файла
        if ((DateTime.UtcNow - fileDateTime) > _currencySetting.CacheExpiration)
            return null;

        string json = await File.ReadAllTextAsync(filePath, cancellationToken);
        var cachedData = JsonSerializer.Deserialize<CurrencyDTO[]>(json);

        if (cachedData != null && (DateTime.UtcNow - File.GetLastWriteTimeUtc(filePath)) < _currencySetting.CacheExpiration)
        {
            return cachedData.FirstOrDefault(x => x.CurrencyType == currencyType);
        }

        return null;
    }
    
    private async Task<CurrencyDTO?> GetCurrencyForDateFromCache(
        string filePath,
        CurrencyType currencyType,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
            return null;

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        if (!TryParseCacheDate(fileName, out var fileDateTime))
        {
            _logger.LogWarning("Не удалось распарсить дату из имени кэш-файла: {FileName}", fileName);
            return null;
        }

        string json = await File.ReadAllTextAsync(filePath, cancellationToken);
        var cachedData = JsonSerializer.Deserialize<CurrencyDTO[]>(json);

        return cachedData?.FirstOrDefault(x => x.CurrencyType == currencyType);
    }

    private bool TryParseCacheDate(string fileName, out DateTime dateTime)
    {
        // Попробуем с временем: "yyyy-MM-dd_HH-mm"
        if (DateTime.TryParseExact(fileName, "yyyy-MM-dd_HH-mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            return true;

        // Попробуем только дату: "yyyy-MM-dd"
        if (DateTime.TryParseExact(fileName, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly))
        {
            dateTime = dateOnly;
            return true;
        }

        return false;
    }

    private CurrencyDTO?[] GetCurrencyDtos(CurrenciesOnDate currenciesOnDate)
    {
        return currenciesOnDate
            .Rates
            .Select(
                x =>
                {
                    var result = ParsingCurrencyCode(x.Code);
                    return new CurrencyDTO(result, x.Value);
                })
            .Where(r => r.CurrencyType != CurrencyType.NotSet)
            .ToArray();
    }

    private CurrencyType ParsingCurrencyCode(string currencyCode)
    {
        return currencyCode switch
        {
            "USD" => CurrencyType.USD,
            "RUB" => CurrencyType.RUB,
            "KZT" => CurrencyType.KZT,
            _ => CurrencyType.NotSet
            //_ => throw new InvalidOperationException($"Неправильное значение кода валюты: '{currencyCode}'")
        };
    }

    private async Task<CurrencyDTO> GetCurrencyDto(CurrenciesOnDate currenciesOnDate, CurrencyType currencyType)
    {
        var currencyDto = GetCurrencyDtos(currenciesOnDate);

        await WriteCacheAsync(currencyDto, currenciesOnDate.Date);
        return currencyDto.Single(x => x.CurrencyType == currencyType);
    }
}