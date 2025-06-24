using Currency;
using General.Enums;
using Fuse8.BackendInternship.PublicApi.Helpers;
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

    public async Task<CurrencyRate> GetCurrencyRateAsync(CurrencyType currencyType, CancellationToken cancellationToken)
    {
        var baseCurrencyCode = CurrencyTypeHelper.ToCurrencyCode(_currencySetting.BaseCurrency);
        var currencyCode = CurrencyTypeHelper.ToCurrencyCode(currencyType);
        var request = new CurrencyRateRequest
        {
            BaseCurrencyCode = baseCurrencyCode,
            CurrencyCode = currencyCode
        };

        var grpcResponse = await _currencyApiClient.GetCurrencyRateAsync(request, cancellationToken: cancellationToken);

        var resultCode = CurrencyTypeHelper.ParsingCurrencyCodeToString(grpcResponse.CurrencyCode);
        var response = new CurrencyRate
        {
            Code = resultCode,
            Value = RoundHelper.RoundCurrencyValue((decimal)grpcResponse.Value, _currencySetting.Accuracy)
        };

        return response;
    }

    public async Task<DatedCurrencyRate> GetCurrencyDataWithRateAsync(
        CurrencyType currencyType,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var baseCurrencyCode = CurrencyTypeHelper.ToCurrencyCode(_currencySetting.BaseCurrency);
        var currencyCode = CurrencyTypeHelper.ToCurrencyCode(currencyType);
        var request = new CurrencyRateOnDateRequest
        {
            BaseCurrencyCode = baseCurrencyCode,
            CurrencyCode = currencyCode,
            Date = new GRPCDateOnly
            {
                Day = date.Day,
                Month = date.Month,
                Year = date.Year
            }
        };

        var response = await _currencyApiClient.GetCurrencyDataWithRateAsync(request, cancellationToken: cancellationToken);

        var resultCode = CurrencyTypeHelper.ParsingCurrencyCodeToString(response.CurrencyCode);
        var datedCurrencyRate = new DatedCurrencyRate()
        {
            Date = date,
            Code = resultCode,
            Value = RoundHelper.RoundCurrencyValue(response.Value, _currencySetting.Accuracy)
        };

        return datedCurrencyRate;
    }

    public async Task<ApiSettings> GetApiSettingsAsync(Empty request, CancellationToken cancellationToken)
    {
        var response = await _currencyApiClient.GetApiSettingsAsync(request, cancellationToken: cancellationToken);

        var settingsApi = new ApiSettings
        {
            DefaultCurrency = _currencySetting.Currency,
            NewRequestsAvailable = response.HasRequestsLeft,
            CurrencyRoundCount = _currencySetting.Accuracy
        };

        return settingsApi;
    }
}