using System.Text.Json;
using Fuse8.BackendInternship.InternalApi.Contracts;
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
        Directory.CreateDirectory(_cacheDirectory); // Создаем папку для кэша, если её нет
    }
    
    public async Task<CurrencyDTO> GetCurrentCurrencyAsync(CurrencyType currencyType, CancellationToken cancellationToken)
    {
        string? latestFile = GetLatestCacheFile();
        if (latestFile != null)
        {
            var cachedData = await ReadCacheAsync<CurrencyDTO>(latestFile);
            if (cachedData != null && (DateTime.UtcNow - File.GetLastWriteTimeUtc(latestFile)) < _cacheExpiration)
            {
                return cachedData;
            }
        }

        // Данных нет или кэш устарел → запрашиваем у API
        var currencyCode = currencyType.ToString().ToUpper();
        var currencies = await _currencyApi.GetAllCurrentCurrenciesAsync(currencyCode, cancellationToken);
        var newCurrency = currencies.FirstOrDefault(c => c.Code == currencyCode);
        if (newCurrency == null)
            throw new Exception($"Currency {currencyType} not found");

        var currencyDto = new CurrencyDTO(currencyType, newCurrency.Value);
        await WriteCacheAsync(currencyDto);
        return currencyDto;
    }

    public async Task<CurrencyDTO> GetCurrencyOnDateAsync(
        CurrencyType currencyType,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        string? dateFile = GetCacheFileForDate(date);
        if (dateFile != null)
        {
            var cachedData = await ReadCacheAsync<CurrencyDTO>(dateFile);
            if (cachedData != null)
            {
                return cachedData;
            }
        }

        // Данных нет → запрашиваем у API
        var currencyCode = currencyType.ToString().ToUpper();
        var currenciesOnDate = await _currencyApi.GetAllCurrenciesOnDateAsync(currencyCode, date, cancellationToken);
        var currency = currenciesOnDate.Rates.FirstOrDefault(c => c.Code == currencyCode);
        if (currency == null)
            throw new Exception($"Currency {currencyType} not found on {date}");

        var currencyDto = new CurrencyDTO(currencyType, currency.Value);
        await WriteCacheAsync(currencyDto, date);
        return currencyDto;
    }
    
    private string? GetLatestCacheFile()
    {
        return Directory.GetFiles(_cacheDirectory)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private string? GetCacheFileForDate(DateOnly date)
    {
        return Directory.GetFiles(_cacheDirectory)
            .Where(f => f.Contains(date.ToString("yyyy-MM-dd")))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private async Task WriteCacheAsync(CurrencyDTO data, DateOnly? date = null)
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
}