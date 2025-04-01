using Grpc.Core;
using InternalApi.Enums;
using InternalApi.Interfaces;
using InternalApi.Settings;
using Microsoft.Extensions.Options;

namespace InternalApi.Services;

public class GrpcService : CurrencyApi.CurrencyApiBase
{
    private readonly ICachedCurrencyAPI _cachedCurrencyApi;

    private readonly ICurrencyApiService _currencyApiService;

    private readonly CurrencySetting _currencySetting;

    public GrpcService(
        ICachedCurrencyAPI cachedCurrencyApi,
        ICurrencyApiService currencyApiService,
        IOptionsSnapshot<CurrencySetting> currencySetting)
    {
        _cachedCurrencyApi = cachedCurrencyApi;
        _currencyApiService = currencyApiService;
        _currencySetting = currencySetting.Value;
    }

    public override async Task<CurrencyRateResponse> GetCurrencyRate(CurrencyRateRequest request, ServerCallContext context)
    {
        Enum.TryParse(request.CurrencyCode, true, out CurrencyType currencyType);
        var currencyData = await _cachedCurrencyApi.GetCurrentCurrencyAsync(currencyType, context.CancellationToken);

        return new CurrencyRateResponse
        {
            CurrencyCode = request.CurrencyCode,
            Value = RoundCurrencyValue((double)currencyData.Value)
        };
    }

    public override async Task<CurrencyRateOnDateResponse> GetCurrencyDataWithRate(
        CurrencyRateOnDateRequest request,
        ServerCallContext context)
    {
        Enum.TryParse(request.CurrencyCode, true, out CurrencyType currencyType);
        var date = DateOnly.Parse(request.Date);
        var currencyData = await _cachedCurrencyApi.GetCurrencyOnDateAsync(currencyType, date, context.CancellationToken);

        return new CurrencyRateOnDateResponse
        {
            Date = request.Date,
            CurrencyCode = request.CurrencyCode,
            Value = RoundCurrencyValue((double)currencyData.Value)
        };
    }

    public override async Task<ApiSettingsResponse> GetApiSettings(ApiSettingsRequest request, ServerCallContext context)
    {
        var settings = await _currencyApiService.GetApiSettingsAsync();

        return new ApiSettingsResponse
        {
            DefaultCurrency = settings.DefaultCurrency,
            BaseCurrency = settings.BaseCurrency,
            RequestLimit = settings.RequestLimit,
            RequestCount = settings.RequestCount,
            HasRequestsLeft = settings.RequestLimit > settings.RequestCount,
            CurrencyRoundCount = settings.CurrencyRoundCount
        };
    }

    private double RoundCurrencyValue(double value) => Math.Round(value, _currencySetting.Accuracy);
}