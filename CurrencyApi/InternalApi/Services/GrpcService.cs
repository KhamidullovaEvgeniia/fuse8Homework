using Fuse8.BackendInternship.InternalApi.Contracts;
using Grpc.Core;
using InternalApi.Enums;
using InternalApi.Interfaces;

namespace InternalApi.Services;

public class GrpcService : CurrencyApi.CurrencyApiBase
{
    private readonly ICachedCurrencyAPI _cachedCurrencyApi;
    private readonly ICurrencyApiService _currencyApiService;

    public GrpcService(ICachedCurrencyAPI cachedCurrencyApi, ICurrencyApiService currencyApiService)
    {
        _cachedCurrencyApi = cachedCurrencyApi;
        _currencyApiService = currencyApiService;
    }

    // Получить текущий курс валюты
    public override async Task<CurrencyRateResponse> GetCurrencyRate(CurrencyRateRequest request, ServerCallContext context)
    {
        var currencyDto = await _cachedCurrencyApi.GetCurrentCurrencyAsync(
            (CurrencyType)Enum.Parse(typeof(CurrencyType), request.CurrencyCode),
            context.CancellationToken);

        return new CurrencyRateResponse { CurrencyCode = currencyDto.CurrencyType.ToString(), Value = (double)currencyDto.Value };
    }

    // Получить курс валюты на определенную дату
    public override async Task<CurrencyRateOnDateResponse> GetCurrencyDataWithRate(
        CurrencyRateOnDateRequest request,
        ServerCallContext context)
    {
        var date = DateOnly.Parse(request.Date);
        var currencyDto = await _cachedCurrencyApi.GetCurrencyOnDateAsync(
            (CurrencyType)Enum.Parse(typeof(CurrencyType), request.CurrencyCode),
            date,
            context.CancellationToken);

        return new CurrencyRateOnDateResponse
        {
            Date = date.ToString("yyyy-MM-dd"),
            CurrencyCode = currencyDto.CurrencyType.ToString(),
            Value = (double)currencyDto.Value
        };
    }

    // Получить настройки API
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
}