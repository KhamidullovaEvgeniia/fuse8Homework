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
    private const HttpStatusCode CurrencyNotFoundHttpStatusCode = (HttpStatusCode)422;

    private readonly HttpClient _httpClient;

    private readonly int _accuracy;

    private readonly string _baseCurrency;

    private readonly string _currency;

    public CurrencyApiService(HttpClient httpClient, IOptions<CurrencySetting> currencySetting)
    {
        _httpClient = httpClient;
        _accuracy = currencySetting.Value.Accuracy;
        _baseCurrency = currencySetting.Value.BaseCurrency;
        _currency = currencySetting.Value.DefaultCurrency;
    }

    public async Task<CurrencyRate> GetCurrencyRateAsync(string currencyCode)
    {
        var url = $"v3/latest?currencies={currencyCode}&base_currency={_baseCurrency}";

        var result = await FetchCurrencyDataAsync(url);
        

        var currencyRate = new CurrencyRate()
        {
            Code = result.Data.First().Value.Code,
            Value = Math.Round(result.Data.First().Value.Value, _accuracy)
        };

        return currencyRate;
    }

    public async Task<DatedCurrencyRate> GetCurrencyDataWithRateAsync(string currencyCode, DateTime date)
    {
        string formattedDate = date.ToString("yyyy-MM-dd");
        var url = $"v3/historical?currencies={currencyCode}&date={formattedDate}&base_currency={_baseCurrency}";

        var result = await FetchCurrencyDataAsync(url);

        var datedCurrencyRate = new DatedCurrencyRate()
        {
            Date = result.Meta.LastUpdatedAt.ToString("yyyy-MM-dd"),
            Code = result.Data.First().Value.Code,
            Value = Math.Round(result.Data.First().Value.Value, _accuracy)
        };

        return datedCurrencyRate;
    }

    public async Task<ApiSettings> GetApiSettingsAsync()
    {
        var result = await GetQuotasResponse();
        CheckRequestLimit(result);

        var settingsApi = new ApiSettings()
        {
            DefaultCurrency = _currency,
            BaseCurrency = _baseCurrency,
            RequestLimit = result.Quotas.Month.Total,
            RequestCount = result.Quotas.Month.Used,
            CurrencyRoundCount = _accuracy
        };

        return settingsApi;
    }

    private void CheckRequestLimit(QuotaResponse result)
    {
        if (result.Quotas.Month.Used == result.Quotas.Month.Total)
            throw new ApiRequestLimitException();
    }

    private async Task GetAndCheckRequestLimit()
    {
        var result = await GetQuotasResponse();
        CheckRequestLimit(result);
    }

    private async Task<QuotaResponse> GetQuotasResponse()
    {
        var url = "v3/status";
        var response = await _httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<QuotaResponse>(content);
        if (result is null)
            throw new NullReferenceException(message: "Deserialize QuotaResponse is null");

        return result;
    }
    
    private async Task<CurrencyResponse> FetchCurrencyDataAsync(string url)
    {
        await GetAndCheckRequestLimit();

        var response = await _httpClient.GetAsync(url);

        if (response.StatusCode == CurrencyNotFoundHttpStatusCode)
            throw new CurrencyNotFoundException();
        
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CurrencyResponse>(content);

        if (result is null)
            throw new NullReferenceException("Deserialize CurrencyResponse is null");

        return result;
    }
}