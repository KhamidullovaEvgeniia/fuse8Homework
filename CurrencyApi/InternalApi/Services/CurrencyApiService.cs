using InternalApi.Enums;
using InternalApi.Interfaces;
using InternalApi.Models;
using InternalApi.Settings;
using Microsoft.Extensions.Options;

namespace InternalApi.Services;

public class CurrencyApiService : ICurrencyApiService, ICurrencyAPI
{
    private readonly ICurrencyHttpApi _currencyHttpApi;

    private readonly CurrencySetting _currencySetting;

    public CurrencyApiService(ICurrencyHttpApi currencyHttpApi, IOptionsSnapshot<CurrencySetting> currencySetting)
    {
        _currencyHttpApi = currencyHttpApi;
        _currencySetting = currencySetting.Value;
    }

    public async Task<CurrencyRates> GetCurrencyRateAsync(string currencyCode)
    {
        var result = await _currencyHttpApi.GetCurrencyRateAsync(currencyCode);

        var resultFirst = result.Data.First().Value;
        var currencyRate = new CurrencyRates() { Code = resultFirst.Code, Value = RoundCurrencyValue(resultFirst.Value) };

        return currencyRate;
    }

    public async Task<DatedCurrencyRate> GetCurrencyDataWithRateAsync(string currencyCode, DateOnly date)
    {
        var result = await _currencyHttpApi.GetCurrencyDataWithRateAsync(currencyCode, date);
        var resultFirst = result.Data.First().Value;

        var datedCurrencyRate = new DatedCurrencyRate()
        {
            Date = date, Code = resultFirst.Code, Value = RoundCurrencyValue(resultFirst.Value)
        };

        return datedCurrencyRate;
    }

    public async Task<ApiSettings> GetApiSettingsAsync()
    {
        var result = await _currencyHttpApi.GetApiQuotasAsync();

        var settingsApi = new ApiSettings()
        {
            DefaultCurrency = _currencySetting.Currency,
            BaseCurrency = _currencySetting.BaseCurrency,
            RequestLimit = result.Quotas.Month.Total,
            RequestCount = result.Quotas.Month.Used,
            CurrencyRoundCount = _currencySetting.Accuracy
        };

        return settingsApi;
    }

    public async Task<CurrencyRates[]> GetAllCurrentCurrenciesAsync(string currencyCode, CancellationToken cancellationToken)
    {
        var result = await _currencyHttpApi.GetCurrencyRateAsync(currencyCode);
        return result.Data.Select(x => new CurrencyRates { Code = x.Value.Code, Value = x.Value.Value }).ToArray();
    }

    public async Task<CurrenciesOnDate> GetAllCurrenciesOnDateAsync(
        string currencyCode,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var result = await _currencyHttpApi.GetCurrencyDataWithRateAsync(currencyCode, date);
        var resultCurrencies = result.Data.Select(x => new CurrencyRates { Code = x.Value.Code, Value = x.Value.Value }).ToArray();
        return new CurrenciesOnDate { Date = date, Rates = resultCurrencies };
    }

    private decimal RoundCurrencyValue(decimal value) => Math.Round(value, _currencySetting.Accuracy);
}