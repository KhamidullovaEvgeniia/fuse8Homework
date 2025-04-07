using System.Globalization;
using System.Text.Json;
using InternalApi.DataAccess.Interfaces;
using InternalApi.DataAccess.Models;
using InternalApi.Enums;
using InternalApi.Filters;
using InternalApi.Interfaces;
using InternalApi.Models;
using InternalApi.Settings;
using Microsoft.Extensions.Options;

namespace InternalApi.Services;

public class CachedCurrencyService : ICachedCurrencyAPI
{
    private readonly IExchangeDateRepository _exchangeDateRepo;

    private readonly ICurrencyRateRepository _currencyRateRepo;

    private readonly ICurrencyAPI _currencyApi;

    private readonly CurrencySetting _currencySetting;

    private readonly ILogger<ExceptionFilter> _logger;

    private readonly CurrencyType _currencyType = CurrencyType.USD;

    public CachedCurrencyService(
        ICurrencyAPI currencyApi,
        IOptionsSnapshot<CurrencySetting> currencySetting,
        ILogger<ExceptionFilter> logger,
        IExchangeDateRepository exchangeDateRepo,
        ICurrencyRateRepository currencyRateRepo)
    {
        _currencyApi = currencyApi;
        _currencySetting = currencySetting.Value;
        Directory.CreateDirectory(_currencySetting.CacheDirectory);
        _logger = logger;
        _exchangeDateRepo = exchangeDateRepo;
        _currencyRateRepo = currencyRateRepo;
    }

    public async Task<CurrencyDTO> GetCurrentCurrencyAsync(CurrencyType currencyType, CancellationToken cancellationToken)
    {
        var requestedDate = DateTime.UtcNow;

        var exchangeDate =
            await _exchangeDateRepo.FindByDateWithinExpirationAsync(requestedDate, _currencySetting.CacheExpiration);

        if (exchangeDate != null)
        {
            
            var apiRates = await _currencyRateRepo.GetByKeyAsync((int)currencyType, exchangeDate.Id);
            if (apiRates is null)
            {
                throw new InvalidOperationException("Could not find currency rates");
            }

            return new CurrencyDTO((CurrencyType)apiRates.Currency, apiRates.Value);
        }

        // Дата не найдена — добавляем новую и запрашиваем API
        var newExchangeDate = await _exchangeDateRepo.AddAsync(requestedDate);
        if (newExchangeDate == null)
            throw new InvalidOperationException("Could not add currency date");

        var currencies = await _currencyApi.GetAllCurrentCurrenciesAsync(_currencyType.ToString(), cancellationToken);
        foreach (var rate in currencies.Rates)
        {
            var d = ParsingCurrencyCode(rate.Code);
            if (d == CurrencyType.NotSet)
                continue;

            var a = new CurrencyRate
            {
                Currency = (int)d,
                Value = rate.Value,
                DateId = newExchangeDate.Id,
                ExchangeDate = newExchangeDate,
            };

            await _currencyRateRepo.AddAsync(a);
        }

        await _exchangeDateRepo.SaveChangesAsync();
        await _currencyRateRepo.SaveChangesAsync();

        return await GetCurrencyDtoFromDb(newExchangeDate.Id, (int)currencyType);
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

    private async Task<CurrencyDTO> GetCurrencyDtoFromDb(int dateId, int currencyType)
    {
        var apiRates = await _currencyRateRepo.GetByKeyAsync(currencyType, dateId);
        return new CurrencyDTO((CurrencyType)apiRates.Currency, apiRates.Value);
    }
}