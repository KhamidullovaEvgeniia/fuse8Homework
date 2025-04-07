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

    public GrpcService(
        ICachedCurrencyAPI cachedCurrencyApi,
        ICurrencyAPI currencyApiService,
        IOptionsSnapshot<CurrencySetting> currencySetting)
    {
        _cachedCurrencyApi = cachedCurrencyApi;
        _currencyApiService = currencyApiService;
    }

    public override async Task<CurrencyRateResponse> GetCurrencyRate(CurrencyRateRequest request, ServerCallContext context)
    {
        Enum.TryParse(request.CurrencyCode, true, out CurrencyType currencyType);
        Enum.TryParse(request.BaseCurrencyCode, true, out CurrencyType baseCurrencyType);

        var currencyData = await _cachedCurrencyApi.GetCurrentCurrencyAsync(
            baseCurrencyType,
            currencyType,
            context.CancellationToken);

        return new CurrencyRateResponse
        {
            CurrencyCode = request.CurrencyCode,
            Value = (double)currencyData.Value,
        };
    }

    public override async Task<CurrencyRateOnDateResponse> GetCurrencyDataWithRate(
        CurrencyRateOnDateRequest request,
        ServerCallContext context)
    {
        Enum.TryParse(request.BaseCurrencyCode, true, out CurrencyType baseCurrencyType);
        Enum.TryParse(request.CurrencyCode, true, out CurrencyType currencyType);

        var grpcDateOnly = request.Date;
        var date = new DateOnly(grpcDateOnly.Year, grpcDateOnly.Month, grpcDateOnly.Day);
        var currencyData = await _cachedCurrencyApi.GetCurrencyOnDateAsync(baseCurrencyType,currencyType, date, context.CancellationToken);

        return new CurrencyRateOnDateResponse
        {
            Date = request.Date,
            CurrencyCode = request.CurrencyCode,
            Value = (double)currencyData.Value
        };
    }

    public override async Task<ApiSettingsResponse> GetApiSettings(Empty request, ServerCallContext context)
    {
        var settings = await _currencyApiService.GetApiSettingsAsync();

        return new ApiSettingsResponse
        {
            HasRequestsLeft = settings.RequestLimit > settings.RequestCount,
        };
    }
}