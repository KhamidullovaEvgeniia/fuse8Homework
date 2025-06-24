using InternalApi.Interfaces;
using InternalApi.Models;
using InternalApi.Responses;

namespace InternalApi.Services;

public class CurrencyApiService : ICurrencyAPI
{
    private readonly ICurrencyHttpApi _currencyHttpApi;

    public CurrencyApiService(ICurrencyHttpApi currencyHttpApi)
    {
        _currencyHttpApi = currencyHttpApi;
    }

    public async Task<ApiSettings> GetApiSettingsAsync(CancellationToken cancellationToken)
    {
        var result = await _currencyHttpApi.GetApiQuotasAsync(cancellationToken: cancellationToken);

        var settingsApi = new ApiSettings() { RequestLimit = result.Quotas.Month.Total, RequestCount = result.Quotas.Month.Used };

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