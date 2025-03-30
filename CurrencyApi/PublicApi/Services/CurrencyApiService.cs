using System.Net;
using System.Text.Json;
using Fuse8.BackendInternship.PublicApi.Exceptions;
using Fuse8.BackendInternship.PublicApi.Interfaces;
using Fuse8.BackendInternship.PublicApi.Models;
using Fuse8.BackendInternship.PublicApi.Responses;
using Fuse8.BackendInternship.PublicApi.Settings;
using Microsoft.Extensions.Options;

namespace Fuse8.BackendInternship.PublicApi.Services;

public class CurrencyApiService : ICurrencyApiService
{
    private readonly ICurrencyHttpApi _currencyHttpApi;

    private readonly CurrencySetting _currencySetting;

    public CurrencyApiService(ICurrencyHttpApi currencyHttpApi, IOptionsSnapshot<CurrencySetting> currencySetting)
    {
        _currencyHttpApi = currencyHttpApi;
        _currencySetting = currencySetting.Value;
    }

    public async Task<CurrencyRate> GetCurrencyRateAsync(string currencyCode)
    {
        var result = await _currencyHttpApi.GetCurrencyRateAsync(currencyCode);

        var resultFirst = result.Data.First().Value;
        var currencyRate = new CurrencyRate() { Code = resultFirst.Code, Value = RoundCurrencyValue(resultFirst.Value) };

        return currencyRate;
    }

    public async Task<DatedCurrencyRate> GetCurrencyDataWithRateAsync(string currencyCode, DateTime date)
    {
        var result = await _currencyHttpApi.GetCurrencyDataWithRateAsync(currencyCode, date);
        var resultFirst = result.Data.First().Value;

        var datedCurrencyRate = new DatedCurrencyRate()
        {
            Date = result.Meta.LastUpdatedAt.ToString("yyyy-MM-dd"),
            Code = resultFirst.Code,
            Value = RoundCurrencyValue(resultFirst.Value)
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

    private decimal RoundCurrencyValue(decimal value) => Math.Round(value, _currencySetting.Accuracy);
}