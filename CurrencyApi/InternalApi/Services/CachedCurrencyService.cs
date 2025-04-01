using System.Text.Json;
using InternalApi.Enums;
using InternalApi.Interfaces;
using InternalApi.Models;

namespace InternalApi.Services;

public class CachedCurrencyService : ICachedCurrencyAPI
{
    private readonly ICurrencyAPI _currencyApi;

    private readonly string _cacheDirectory = "CurrencyCache";

    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(2);

    public CachedCurrencyService(ICurrencyAPI currencyApi)
    {
        _currencyApi = currencyApi;
        Directory.CreateDirectory(_cacheDirectory);
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

        var currencyCode = currencyType.ToString().ToUpper();
        var currencies = await _currencyApi.GetAllCurrentCurrenciesAsync(currencyCode, cancellationToken);

        var currencyDto = currencies
            .Select(
                x =>
                {
                    Enum.TryParse(x.Code, true, out CurrencyType result);
                    return new CurrencyDTO(result, x.Value);
                })
            .ToArray();

        await WriteCacheAsync(currencyDto);
        return currencyDto.First(x => x.CurrencyType == currencyType);
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

        var currencyCode = currencyType.ToString().ToUpper();
        var currenciesOnDate = await _currencyApi.GetAllCurrenciesOnDateAsync(currencyCode, date, cancellationToken);

        var currencyDto = currenciesOnDate
            .Rates
            .Select(
                x =>
                {
                    Enum.TryParse(x.Code, true, out CurrencyType result);
                    return new CurrencyDTO(result, x.Value);
                })
            .ToArray();

        await WriteCacheAsync(currencyDto, date);
        return currencyDto.First(x => x.CurrencyType == currencyType);
    }

    private string? GetLatestCacheFile()
    {
        return Directory.GetFiles(_cacheDirectory).OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
    }

    private string? GetCacheFileForDate(DateOnly date)
    {
        return Directory
            .GetFiles(_cacheDirectory)
            .Where(f => f.Contains(date.ToString("yyyy-MM-dd")))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private async Task WriteCacheAsync(CurrencyDTO[] data, DateOnly? date = null)
    {
        string fileName = date?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm");
        string filePath = Path.Combine(_cacheDirectory, $"{fileName}.json");
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
    }

    private async Task<T?> ReadCacheAsync<T>(string? filePath)
    {
        if (!File.Exists(filePath))
            return default;

        string json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(json);
    }

    private async Task<CurrencyDTO?> GetCurrencyFromCache(
        string filePath,
        CurrencyType currencyType,
        CancellationToken cancellationToken)
    {
        var cachedData = await ReadCacheAsync<CurrencyDTO[]>(filePath);
        if (cachedData != null && (DateTime.UtcNow - File.GetLastWriteTimeUtc(filePath)) < _cacheExpiration)
        {
            return cachedData.First(x => x.CurrencyType == currencyType);
        }

        return default;
    }
}