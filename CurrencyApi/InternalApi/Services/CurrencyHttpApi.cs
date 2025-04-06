using System.Net;
using System.Text.Json;
using InternalApi.Exceptions;
using InternalApi.Interfaces;
using InternalApi.Responses;
using Microsoft.Extensions.Options;
using InternalApi.Settings;


namespace InternalApi.Services;

public class CurrencyHttpApi : ICurrencyHttpApi
{
    private const string CurrenciesQueryKey = "currencies";

    private const string BaseCurrenciesQueryKey = "base_currency";

    private readonly HttpClient _httpClient;

    private readonly CurrencySetting _currencySetting;

    public CurrencyHttpApi(HttpClient httpClient, IOptionsSnapshot<CurrencySetting> currencySetting)
    {
        _httpClient = httpClient;
        _currencySetting = currencySetting.Value;
    }

    public async Task<CurrencyResponse> GetCurrencyRateAsync(string currencyCode)
    {
        var url = $"latest?{CurrenciesQueryKey}={currencyCode}&{BaseCurrenciesQueryKey}={_currencySetting.BaseCurrency}";

        var result = await FetchCurrencyDataAsync(url);

        return result;
    }

    public async Task<CurrencyResponse> GetCurrencyDataWithRateAsync(string currencyCode, DateOnly date)
    {
        string formattedDate = date.ToString("yyyy-MM-dd");
        var url =
            $"historical?{CurrenciesQueryKey}={currencyCode}&date={formattedDate}&{BaseCurrenciesQueryKey}={_currencySetting.BaseCurrency}";

        var result = await FetchCurrencyDataAsync(url);

        return result;
    }

    public async Task<QuotaResponse> GetApiQuotasAsync()
    {
        var result = await GetQuotasResponse();

        return result;
    }
    
    public async Task<CurrencyResponse> GetAllCurrenciesRateAsync(string baseCurrency)
    {
        var url = $"latest?{BaseCurrenciesQueryKey}={baseCurrency}";

        var result = await FetchCurrencyDataAsync(url);

        return result;
    }
    
    public async Task<CurrencyResponse> GetAllCurrenciesDataWithRateAsync(string baseCurrency, DateOnly date)
    {
        string formattedDate = date.ToString("yyyy-MM-dd");
        var url =
            $"historical?date={formattedDate}&{BaseCurrenciesQueryKey}={baseCurrency}";

        var result = await FetchCurrencyDataAsync(url);

        return result;
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
        var url = "status";
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

        if (response.StatusCode is HttpStatusCode.UnprocessableEntity)
        {
            var validationError = await response.Content.ReadFromJsonAsync<ErrorApiResponse>();
            if (validationError is { Message: "Validation error", ErrorsByRequestFieldName: var errors })
            {
                // При передаче неизвестной валюты, api возвращает ErrorApiResponse с конкретным сообщением.
                // Преобразуем это сообщение в CurrencyNotFoundException
                if (errors.TryGetValue(CurrenciesQueryKey, out var currenciesErrors)
                    && currenciesErrors.Contains("The selected currencies is invalid."))
                {
                    throw new CurrencyNotFoundException();
                }
            }
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CurrencyResponse>(content);

        if (result is null)
            throw new NullReferenceException("Deserialize CurrencyResponse is null");

        return result;
    }
}