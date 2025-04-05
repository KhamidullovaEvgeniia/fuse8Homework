using Currency;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InternalApi.Enums;
using InternalApi.Interfaces;
using InternalApi.Settings;
using Microsoft.Extensions.Options;
using Enum = System.Enum;

namespace InternalApi.Services;

public class GrpcService : CurrencyApi.CurrencyApiBase
{
    private readonly ICachedCurrencyAPI _cachedCurrencyApi;

    private readonly ICurrencyAPI _currencyApiService;

    private readonly CurrencySetting _currencySetting;

    public GrpcService(
        ICachedCurrencyAPI cachedCurrencyApi,
        ICurrencyAPI currencyApiService,
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

        var grpcDateOnly = request.Date;
        var date = new DateOnly(grpcDateOnly.Year, grpcDateOnly.Month, grpcDateOnly.Day);
        var currencyData = await _cachedCurrencyApi.GetCurrencyOnDateAsync(currencyType, date, context.CancellationToken);

        return new CurrencyRateOnDateResponse
        {
            Date = request.Date,
            CurrencyCode = request.CurrencyCode,
            Value = RoundCurrencyValue((double)currencyData.Value)
        };
    }

    public override async Task<ApiSettingsResponse> GetApiSettings(Empty request, ServerCallContext context)
    {
        var settings = await _currencyApiService.GetApiSettingsAsync();

        return new ApiSettingsResponse
        {
            BaseCurrency = settings.BaseCurrency,
            HasRequestsLeft = settings.RequestLimit > settings.RequestCount,
        };
    }

    private double RoundCurrencyValue(double value) => Math.Round(value, _currencySetting.Accuracy);
    
}