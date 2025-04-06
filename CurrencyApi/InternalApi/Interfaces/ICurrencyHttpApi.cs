

using InternalApi.Responses;

namespace InternalApi.Interfaces;
// TODO: удалить неиспользуемое
public interface ICurrencyHttpApi
{
    Task<CurrencyResponse> GetCurrencyRateAsync(string currencyCode);

    Task<CurrencyResponse> GetCurrencyDataWithRateAsync(string currencyCode, DateOnly date);

    Task<QuotaResponse> GetApiQuotasAsync();

    Task<CurrencyResponse> GetAllCurrenciesRateAsync(string baseCurrency);

    Task<CurrencyResponse> GetAllCurrenciesDataWithRateAsync(string currencyCode, DateOnly date);
}