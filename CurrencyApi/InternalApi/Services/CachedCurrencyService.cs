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

    private readonly CurrencyType _currencyType = CurrencyType.USD;

    public CachedCurrencyService(
        ICurrencyAPI currencyApi,
        IOptionsSnapshot<CurrencySetting> currencySetting,
        IExchangeDateRepository exchangeDateRepo,
        ICurrencyRateRepository currencyRateRepo)
    {
        _currencyApi = currencyApi;
        _currencySetting = currencySetting.Value;
        Directory.CreateDirectory(_currencySetting.CacheDirectory);

        _exchangeDateRepo = exchangeDateRepo;
        _currencyRateRepo = currencyRateRepo;
    }

    public async Task<CurrencyDTO> GetCurrentCurrencyAsync(
        CurrencyType baseCurrencyType,
        CurrencyType currencyType,
        CancellationToken cancellationToken)
    {
        var exchangeDate =
            await _exchangeDateRepo.FindByDateWithinExpirationAsync(DateTime.UtcNow, _currencySetting.CacheExpiration);

        if (exchangeDate != null)
        {
            return await GetCurrencyDTOAsync((int)baseCurrencyType, (int)currencyType, exchangeDate.Id);
        }

        var currencies = await _currencyApi.GetAllCurrentCurrenciesAsync(_currencyType.ToString(), cancellationToken);
        
        exchangeDate =
            await _exchangeDateRepo.FindByDateWithinExpirationAsync(currencies.Date, _currencySetting.CacheExpiration);

        if (exchangeDate != null)
        {
            return await GetCurrencyDTOAsync((int)baseCurrencyType, (int)currencyType, exchangeDate.Id);
        }

        var newExchangeDate = await SaveCurrencyRatesAsync(currencies.Rates, currencies.Date);

        return await GetCurrencyDTOAsync((int)baseCurrencyType, (int)currencyType, newExchangeDate.Id);
    }

    public async Task<CurrencyDTO> GetCurrencyOnDateAsync(
        CurrencyType baseCurrencyType,
        CurrencyType currencyType,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var dateTime = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
        var exchangeDate = await _exchangeDateRepo.FindByDateWithinExpirationAsync(dateTime, _currencySetting.CacheExpiration);

        if (exchangeDate != null)
        {
            return await GetCurrencyDTOAsync((int)baseCurrencyType, (int)currencyType, exchangeDate.Id);
        }

        var currenciesOnDate = await _currencyApi.GetAllCurrenciesOnDateAsync(_currencyType.ToString(), date, cancellationToken);

        var newExchangeDate = await SaveCurrencyRatesAsync(currenciesOnDate.Rates, currenciesOnDate.Date);

        return await GetCurrencyDTOAsync((int)baseCurrencyType, (int)currencyType, newExchangeDate.Id);
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

    private async Task<CurrencyRate> GetCurrencyRateFromDbAsync(int currencyType, int dateId)
    {
        var currentCurrencyRate = await _currencyRateRepo.GetByKeyAsync(currencyType, dateId);
        if (currentCurrencyRate is null)
        {
            throw new InvalidOperationException("Could not find currency rates");
        }

        return currentCurrencyRate;
    }

    private async Task<CurrencyDTO> GetCurrencyDTOAsync(int baseCurrencyType, int currencyType, int dateId)
    {
        var currentCurrencyRate = await GetCurrencyRateFromDbAsync(currencyType, dateId);

        var baseCurrencyRate = await GetCurrencyRateFromDbAsync(baseCurrencyType, dateId);

        var resultCurrencyValue = currentCurrencyRate.Value / baseCurrencyRate.Value;

        return new CurrencyDTO((CurrencyType)currentCurrencyRate.Currency, resultCurrencyValue);
    }

    private async Task<ExchangeDate> SaveCurrencyRatesAsync(IEnumerable<CurrencyRates> rates, DateTime dateTime)
    {
        var newExchangeDate = await _exchangeDateRepo.AddAsync(dateTime);
        if (newExchangeDate == null)
            throw new InvalidOperationException("Could not add currency date");

        foreach (var rate in rates)
        {
            var currencyType = ParsingCurrencyCode(rate.Code);
            if (currencyType == CurrencyType.NotSet)
                continue;

            var currencyRate = new CurrencyRate
            {
                Currency = (int)currencyType,
                Value = rate.Value,
                DateId = newExchangeDate.Id,
                ExchangeDate = newExchangeDate,
            };

            await _currencyRateRepo.AddAsync(currencyRate);
        }

        await _exchangeDateRepo.SaveChangesAsync();
        await _currencyRateRepo.SaveChangesAsync();

        return newExchangeDate;
    }
}