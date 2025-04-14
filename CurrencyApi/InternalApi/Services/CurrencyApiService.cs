using InternalApi.Interfaces;
using InternalApi.Models;
using InternalApi.Responses;
using InternalApi.Settings;
using Microsoft.Extensions.Options;

namespace InternalApi.Services;

public class CurrencyApiService : ICurrencyAPI
{
    private readonly ICurrencyHttpApi _currencyHttpApi;

    private readonly CurrencySetting _currencySetting;

    public CurrencyApiService(ICurrencyHttpApi currencyHttpApi, IOptionsSnapshot<CurrencySetting> currencySetting)
    {
        _currencyHttpApi = currencyHttpApi;
        _currencySetting = currencySetting.Value;
    }

    public async Task<ApiSettings> GetApiSettingsAsync(CancellationToken cancellationToken)
    {
        var result = await _currencyHttpApi.GetApiQuotasAsync(cancellationToken: cancellationToken);

        var settingsApi = new ApiSettings()
        {
            DefaultCurrency = _currencySetting.Currency,
            BaseCurrency = _currencySetting.BaseCurrency,
            RequestLimit = result.Quotas.Month.Total,
            RequestCount = result.Quotas.Month.Used
        };

        return settingsApi;
    }

    public async Task<CurrenciesOnDate> GetAllCurrentCurrenciesAsync(string baseCurrency, CancellationToken cancellationToken)
    {
        var result = await _currencyHttpApi.GetAllCurrenciesRateAsync(baseCurrency, cancellationToken);
        var resultCurrencies = GetAllCurrenciesRates(result);

        return new CurrenciesOnDate { Date = result.Meta.LastUpdatedAt, Rates = resultCurrencies };
    }

    public async Task<CurrenciesOnDate> GetAllCurrenciesOnDateAsync(
        string currencyCode,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var result = await _currencyHttpApi.GetAllCurrenciesDataWithRateAsync(currencyCode, date, cancellationToken);
        var resultCurrencies = GetAllCurrenciesRates(result);

        return new CurrenciesOnDate { Date = result.Meta.LastUpdatedAt, Rates = resultCurrencies };
    }

    private CurrencyRates[] GetAllCurrenciesRates(CurrencyResponse currencyResponse)
    {
        return currencyResponse.Data.Values.Select(x => new CurrencyRates { Code = x.Code, Value = x.Value }).ToArray();
    }
}