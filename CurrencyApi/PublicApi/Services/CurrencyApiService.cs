using Currency;
using Fuse8.BackendInternship.PublicApi.Interfaces;
using Fuse8.BackendInternship.PublicApi.Models;
using Fuse8.BackendInternship.PublicApi.Settings;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;

namespace Fuse8.BackendInternship.PublicApi.Services;

public class CurrencyApiService : ICurrencyApiService
{
    private readonly CurrencySetting _currencySetting;

    private readonly CurrencyApi.CurrencyApiClient _currencyApiClient;

    public CurrencyApiService(IOptionsSnapshot<CurrencySetting> currencySetting, CurrencyApi.CurrencyApiClient currencyApiClient)
    {
        _currencyApiClient = currencyApiClient;
        _currencySetting = currencySetting.Value;
    }

    public async Task<CurrencyRate> GetCurrencyRateAsync(string currencyCode, CancellationToken cancellationToken)
    {
        var request = new CurrencyRateRequest
        {
            BaseCurrencyCode = _currencySetting.BaseCurrency,
            CurrencyCode = currencyCode
        };

        var grpcResponse = await _currencyApiClient.GetCurrencyRateAsync(request);

        var response = new CurrencyRate
        {
            Code = grpcResponse.CurrencyCode,
            Value = Helpers.CurrencyHelper.RoundCurrencyValue((decimal)grpcResponse.Value, _currencySetting.Accuracy)
        };

        return response;
    }

    public async Task<DatedCurrencyRate> GetCurrencyDataWithRateAsync(
        string currencyCode,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var request = new CurrencyRateOnDateRequest
        {
            BaseCurrencyCode = _currencySetting.BaseCurrency,
            CurrencyCode = currencyCode,
            Date = new GRPCDateOnly
            {
                Day = date.Day,
                Month = date.Month,
                Year = date.Year
            }
        };

        var response = await _currencyApiClient.GetCurrencyDataWithRateAsync(request);

        var datedCurrencyRate = new DatedCurrencyRate()
        {
            Date = date,
            Code = response.CurrencyCode,
            Value = Helpers.CurrencyHelper.RoundCurrencyValue(response.Value, _currencySetting.Accuracy)
        };

        return datedCurrencyRate;
    }

    public async Task<ApiSettings> GetApiSettingsAsync(Empty request)
    {
        var response = await _currencyApiClient.GetApiSettingsAsync(request);

        var settingsApi = new ApiSettings
        {
            DefaultCurrency = _currencySetting.Currency,
            NewRequestsAvailable = response.HasRequestsLeft,
            CurrencyRoundCount = _currencySetting.Accuracy
        };

        return settingsApi;
    }
}