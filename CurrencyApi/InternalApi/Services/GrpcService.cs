using Currency;
using Framework.Enums;
using Framework.Helper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InternalApi.Helpers;
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
        ICurrencyAPI currencyApiService)
    {
        _cachedCurrencyApi = cachedCurrencyApi;
        _currencyApiService = currencyApiService;
    }

    public override async Task<CurrencyRateResponse> GetCurrencyRate(CurrencyRateRequest request, ServerCallContext context)
    {
        var baseCurrencyType = CurrencyTypeHelper.ParsingCurrencyCode(request.BaseCurrencyCode);
        var currencyType = CurrencyTypeHelper.ParsingCurrencyCode(request.CurrencyCode);

        var currencyData = await _cachedCurrencyApi.GetCurrentCurrencyAsync(
            baseCurrencyType,
            currencyType,
            context.CancellationToken);

        return new CurrencyRateResponse
        {
            CurrencyCode = request.CurrencyCode,
            Value = (DecimalValue)currencyData.Value,
        };
    }

    public override async Task<CurrencyRateOnDateResponse> GetCurrencyDataWithRate(
        CurrencyRateOnDateRequest request,
        ServerCallContext context)
    {
        var baseCurrencyType = CurrencyTypeHelper.ParsingCurrencyCode(request.BaseCurrencyCode);
        var currencyType = CurrencyTypeHelper.ParsingCurrencyCode(request.CurrencyCode);

        var grpcDateOnly = request.Date;
        var date = new DateOnly(grpcDateOnly.Year, grpcDateOnly.Month, grpcDateOnly.Day);
        var currencyData = await _cachedCurrencyApi.GetCurrencyOnDateAsync(baseCurrencyType,currencyType, date, context.CancellationToken);
        
        return new CurrencyRateOnDateResponse
        {
            Date = request.Date,
            CurrencyCode = request.CurrencyCode,
            Value = (DecimalValue)currencyData.Value
        };
    }

    public override async Task<ApiSettingsResponse> GetApiSettings(Empty request, ServerCallContext context)
    {
        var settings = await _currencyApiService.GetApiSettingsAsync(context.CancellationToken);

        return new ApiSettingsResponse
        {
            HasRequestsLeft = settings.RequestLimit > settings.RequestCount,
        };
    }
}