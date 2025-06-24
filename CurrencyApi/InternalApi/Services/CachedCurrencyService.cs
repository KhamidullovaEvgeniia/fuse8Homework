using General.Enums;
using General.Helper;
using InternalApi.DataAccess.Interfaces;
using InternalApi.DataAccess.Models;
using InternalApi.Interfaces;
using InternalApi.Models;
using InternalApi.Settings;
using Microsoft.Extensions.Options;

namespace InternalApi.Services;

public class CachedCurrencyService : ICachedCurrencyAPI
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly ICurrencyAPI _currencyApi;

    private readonly CurrencySetting _currencySetting;

    private const CurrencyType _currencyType = CurrencyType.USD;

    public CachedCurrencyService(
        ICurrencyAPI currencyApi,
        IOptionsSnapshot<CurrencySetting> currencySetting,
        IUnitOfWork unitOfWork)
    {
        _currencyApi = currencyApi;
        _currencySetting = currencySetting.Value;

        _unitOfWork = unitOfWork;
    }

    public async Task<CurrencyDTO> GetCurrentCurrencyAsync(
        CurrencyType baseCurrencyType,
        CurrencyType currencyType,
        CancellationToken cancellationToken)
    {
        var exchangeDate = await _unitOfWork.ExchangeDateRepository.FindByDateWithinExpirationAsync(
            DateTime.UtcNow,
            _currencySetting.CacheExpiration,
            cancellationToken);

        if (exchangeDate == null)
        {
            var currencies = await _currencyApi.GetAllCurrentCurrenciesAsync(_currencyType.ToString(), cancellationToken);
            exchangeDate = await SaveCurrencyRatesAsync(currencies.Rates, DateTime.UtcNow, cancellationToken);
        }

        if (baseCurrencyType == _currencyType)
        {
            var currencyRate = await GetCurrencyRateFromDbAsync((int)currencyType, exchangeDate.Id, cancellationToken);
            var resultValue = currencyRate.Value;
            return new CurrencyDTO((CurrencyType)currencyRate.Currency, resultValue);
        }
        else
        {
            var currencyRates = await GetCurrencyRatesFromDbAsync(
                [(int)currencyType, (int)baseCurrencyType],
                exchangeDate.Id,
                cancellationToken);

            var currentRate = currencyRates.FirstOrDefault(r => r.Currency == (int)currencyType);
            var baseRate = currencyRates.FirstOrDefault(r => r.Currency == (int)baseCurrencyType);

            if (currentRate == null || baseRate == null)
                throw new Exception("Не удалось получить нужные курсы из БД.");

            var resultValue = currentRate.Value / baseRate.Value;
            return new CurrencyDTO((CurrencyType)currentRate.Currency, resultValue);
        }
    }

    public async Task<CurrencyDTO> GetCurrencyOnDateAsync(
        CurrencyType baseCurrencyType,
        CurrencyType currencyType,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var dateTime = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
        var exchangeDate = await _unitOfWork.ExchangeDateRepository.FindByDateWithinExpirationAsync(
            dateTime,
            _currencySetting.CacheExpiration,
            cancellationToken);

        if (exchangeDate == null)
        {
            var currencies = await _currencyApi.GetAllCurrenciesOnDateAsync(_currencyType.ToString(), date, cancellationToken);
            exchangeDate = await SaveCurrencyRatesAsync(currencies.Rates, dateTime, cancellationToken);
        }

        if (baseCurrencyType == _currencyType)
        {
            var currencyRate = await GetCurrencyRateFromDbAsync((int)currencyType, exchangeDate.Id, cancellationToken);
            var resultValue = currencyRate.Value;
            return new CurrencyDTO((CurrencyType)currencyRate.Currency, resultValue);
        }
        else
        {
            var currencyRates = await GetCurrencyRatesFromDbAsync(
                [(int)currencyType, (int)baseCurrencyType],
                exchangeDate.Id,
                cancellationToken);

            var currentRate = currencyRates.FirstOrDefault(r => r.Currency == (int)currencyType);
            var baseRate = currencyRates.FirstOrDefault(r => r.Currency == (int)baseCurrencyType);

            if (currentRate == null || baseRate == null)
                throw new Exception("Не удалось получить нужные курсы из БД.");

            var resultValue = currentRate.Value / baseRate.Value;
            return new CurrencyDTO((CurrencyType)currentRate.Currency, resultValue);
        }
    }

    private async Task<CurrencyRate> GetCurrencyRateFromDbAsync(int currencyType, int dateId, CancellationToken cancellationToken)
    {
        var currentCurrencyRate = await _unitOfWork.CurrencyRateRepository.GetByKeyAsync(currencyType, dateId, cancellationToken);
        if (currentCurrencyRate is null)
            throw new InvalidOperationException("Could not find currency rates");

        return currentCurrencyRate;
    }

    private async Task<List<CurrencyRate>> GetCurrencyRatesFromDbAsync(
        IEnumerable<int> currencyTypes,
        int exchangeDateId,
        CancellationToken cancellationToken)
    {
        var currentCurrencyRates =
            await _unitOfWork.CurrencyRateRepository.GetByCurrencyTypesAndDateAsync(
                currencyTypes,
                exchangeDateId,
                cancellationToken);

        if (currentCurrencyRates.Count < 2)
            throw new InvalidOperationException("Could not find currency rates");

        return currentCurrencyRates;
    }

    private async Task<ExchangeDate> SaveCurrencyRatesAsync(
        IEnumerable<CurrencyRates> rates,
        DateTime dateTime,
        CancellationToken cancellationToken)
    {
        var newExchangeDate = await _unitOfWork.ExchangeDateRepository.AddAsync(dateTime, cancellationToken);
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
                Currency = (int)currencyType, Value = rate.Value, DateId = newExchangeDate.Id, ExchangeDate = newExchangeDate,
            };

            await _unitOfWork.CurrencyRateRepository.AddAsync(currencyRate, cancellationToken);
        }

        _unitOfWork.Commit();

        return newExchangeDate;
    }
}