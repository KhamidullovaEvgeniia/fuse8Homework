﻿using System.Net;
using System.Text.Json;
using General.Exceptions;
using InternalApi.Interfaces;
using InternalApi.Responses;

namespace InternalApi.Services;

public class CurrencyHttpApi : ICurrencyHttpApi
{
    private const string CurrenciesQueryKey = "currencies";

    private const string BaseCurrenciesQueryKey = "base_currency";

    private readonly HttpClient _httpClient;

    public CurrencyHttpApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<QuotaResponse> GetApiQuotasAsync(CancellationToken cancellationToken)
    {
        var result = await GetQuotasResponseAsync(cancellationToken);

        return result;
    }

    public async Task<CurrencyResponse> GetAllCurrenciesRateAsync(string baseCurrency, CancellationToken cancellationToken)
    {
        var url = $"latest?{BaseCurrenciesQueryKey}={baseCurrency}";

        var result = await FetchCurrencyDataAsync(url, cancellationToken);

        return result;
    }

    public async Task<CurrencyResponse> GetAllCurrenciesDataWithRateAsync(
        string baseCurrency,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        string formattedDate = date.ToString("yyyy-MM-dd");
        var url = $"historical?date={formattedDate}&{BaseCurrenciesQueryKey}={baseCurrency}";

        var result = await FetchCurrencyDataAsync(url, cancellationToken);

        return result;
    }

    private void CheckRequestLimit(QuotaResponse result)
    {
        if (result.Quotas.Month.Used == result.Quotas.Month.Total)
            throw new ApiRequestLimitException();
    }

    private async Task GetAndCheckRequestLimitAsync(CancellationToken cancellationToken)
    {
        var result = await GetQuotasResponseAsync(cancellationToken);
        CheckRequestLimit(result);
    }

    private async Task<QuotaResponse> GetQuotasResponseAsync(CancellationToken cancellationToken)
    {
        var url = "status";
        var response = await _httpClient.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = JsonSerializer.Deserialize<QuotaResponse>(content);
        if (result is null)
            throw new NullReferenceException(message: "Deserialize QuotaResponse is null");

        return result;
    }

    private async Task<CurrencyResponse> FetchCurrencyDataAsync(string url, CancellationToken cancellationToken)
    {
        await GetAndCheckRequestLimitAsync(cancellationToken);

        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode is HttpStatusCode.UnprocessableEntity)
        {
            var validationError = await response.Content.ReadFromJsonAsync<ErrorApiResponse>(cancellationToken);
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

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<CurrencyResponse>(content);

        if (result is null)
            throw new NullReferenceException("Deserialize CurrencyResponse is null");

        return result;
    }
}