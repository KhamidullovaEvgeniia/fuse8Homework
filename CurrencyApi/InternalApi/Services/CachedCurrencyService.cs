using System.Globalization;
using System.Text.Json;
using Framework.Enums;
using Framework.Helper;
using InternalApi.DataAccess.Interfaces;
using InternalApi.DataAccess.Models;
using InternalApi.Interfaces;
using InternalApi.Models;
using InternalApi.Settings;
using Microsoft.Extensions.Options;

namespace InternalApi.Services;

public class CachedCurrencyService : ICachedCurrencyAPI
{
    private readonly IExchangeDateRepository _exchangeDateRepository;

    private readonly ICurrencyRateRepository _currencyRateRepository;

    private readonly ICurrencyAPI _currencyApi;

    private readonly CurrencySetting _currencySetting;

    private const CurrencyType _currencyType = CurrencyType.USD;

    public CachedCurrencyService(
        ICurrencyAPI currencyApi,
        IOptionsSnapshot<CurrencySetting> currencySetting,
        IExchangeDateRepository exchangeDateRepository,
        ICurrencyRateRepository currencyRateRepository)
    {
        _currencyApi = currencyApi;
        _currencySetting = currencySetting.Value;
        Directory.CreateDirectory(_currencySetting.CacheDirectory);

        _exchangeDateRepository = exchangeDateRepository;
        _currencyRateRepository = currencyRateRepository;
    }

    public async Task<CurrencyDTO> GetCurrentCurrencyAsync(
        CurrencyType baseCurrencyType,
        CurrencyType currencyType,
        CancellationToken cancellationToken)
    {
        var exchangeDate = await _exchangeDateRepository.FindByDateWithinExpirationAsync(
            DateTime.UtcNow,
            _currencySetting.CacheExpiration);

        if (exchangeDate != null)
        {
            return await GetCurrencyDTOAsync((int)baseCurrencyType, (int)currencyType, exchangeDate.Id);
        }

        var currencies = await _currencyApi.GetAllCurrentCurrenciesAsync(_currencyType.ToString(), cancellationToken);

        // Так как в данный момент внешний API, если запросить текущий курс, возвращает дату предыдущего дня со временем 23.59.59,
        // то при запросе в любой момент времени текущего дня, сначала выполняется запрос к базе с текущим временем, если кэша нет,
        // то выполняется запрос к API, далее проверяется есть ли в БД запись даты, которую вернул API
        // В теории API не обязательно вернет дату предыдущего дня со временем 23.59.59, но по моим тестам это пока что так,
        // поэтому реализовано немного неудобно
        exchangeDate = await _exchangeDateRepository.FindByDateWithinExpirationAsync(
            currencies.Date,
            _currencySetting.CacheExpiration);

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
        var exchangeDate =
            await _exchangeDateRepository.FindByDateWithinExpirationAsync(dateTime, _currencySetting.CacheExpiration);

        if (exchangeDate != null)
        {
            return await GetCurrencyDTOAsync((int)baseCurrencyType, (int)currencyType, exchangeDate.Id);
        }

        var currenciesOnDate = await _currencyApi.GetAllCurrenciesOnDateAsync(_currencyType.ToString(), date, cancellationToken);

        var newExchangeDate = await SaveCurrencyRatesAsync(currenciesOnDate.Rates, currenciesOnDate.Date);

        return await GetCurrencyDTOAsync((int)baseCurrencyType, (int)currencyType, newExchangeDate.Id);
    }

    private async Task<CurrencyRate> GetCurrencyRateFromDbAsync(int currencyType, int dateId)
    {
        var currentCurrencyRate = await _currencyRateRepository.GetByKeyAsync(currencyType, dateId);
        if (currentCurrencyRate is null)
            throw new InvalidOperationException("Could not find currency rates");

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
        var newExchangeDate = await _exchangeDateRepository.AddAsync(dateTime);
        if (newExchangeDate == null)
            throw new InvalidOperationException("Could not add currency date");

        foreach (var rate in rates)
        {
            CurrencyType currencyType;

            try
            {
                currencyType = CurrencyHelper.ParsingCurrencyCode(rate.Code);
            }
            catch (InvalidOperationException)
            {
                continue;
            }

            var currencyRate = new CurrencyRate
            {
                Currency = (int)currencyType,
                Value = rate.Value,
                DateId = newExchangeDate.Id,
                ExchangeDate = newExchangeDate,
            };

            await _currencyRateRepository.AddAsync(currencyRate);
        }

        await _exchangeDateRepository.SaveChangesAsync();
        await _currencyRateRepository.SaveChangesAsync();

        return newExchangeDate;
    }
}