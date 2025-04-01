

using InternalApi.Responses;

namespace InternalApi.Interfaces;

public interface ICurrencyHttpApi
{
    Task<CurrencyResponse> GetCurrencyRateAsync(string currencyCode);

    Task<CurrencyResponse> GetCurrencyDataWithRateAsync(string currencyCode, DateOnly date);

    Task<QuotaResponse> GetApiQuotasAsync();

    Task<CurrencyResponse> GetAllCurrenciesRateAsync();

    Task<CurrencyResponse> GetAllCurrenciesDataWithRateAsync(string currencyCode, DateOnly date);
}