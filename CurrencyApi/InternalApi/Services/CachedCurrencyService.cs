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

        var currencyDto = currencies
            .Rates
            .Select(
                x =>
                {
                    Enum.TryParse(x.Code, true, out CurrencyType result);
                    return new CurrencyDTO(result, x.Value);
                })
            .ToArray();

        await WriteCacheAsync(currencyDto, currencies.Date);
        return currencyDto.Single(x => x.CurrencyType == currencyType);
    }

    public async Task<CurrencyDTO> GetCurrencyOnDateAsync(
        CurrencyType currencyType,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        string? dateFile = GetCacheFileForDate(date);
        if (dateFile is not null)
        {
            var currencyFromCache = await GetCurrencyFromCache(dateFile, currencyType, cancellationToken);
            if (currencyFromCache is not null)
                return currencyFromCache;
        }

        var baseCurrency = _currencySetting.BaseCurrency;
        var currenciesOnDate = await _currencyApi.GetAllCurrenciesOnDateAsync(baseCurrency, date, cancellationToken);

        var currencyDto = currenciesOnDate
            .Rates
            .Select(
                x =>
                {
                    if (!Enum.TryParse<CurrencyType>(x.Code, true, out var result)
                        || !Enum.IsDefined(typeof(CurrencyType), result))
                    {
                        _logger.LogWarning("Пропущена неизвестная валюта из API: {Code}", x.Code);
                        return null;
                    }

                    return new CurrencyDTO(result, x.Value);
                })
            .ToArray();

        await WriteCacheAsync(currencyDto, currenciesOnDate.Date);
        return currencyDto.First(x => x.CurrencyType == currencyType);
    }

    private string? GetLatestCacheFile()
    {
        return Directory.GetFiles(_currencySetting.CacheDirectory).OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
    }

    private string? GetCacheFileForDate(DateOnly date)
    {
        var datePart = date.ToString("yyyy-MM-dd");
        return Directory
            .GetFiles(_currencySetting.CacheDirectory)
            .Where(f => f.Contains(datePart))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private async Task WriteCacheAsync(CurrencyDTO[] data, DateTime date)
    {
        string fileName = date.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm");
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
}