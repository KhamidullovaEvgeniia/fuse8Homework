using InternalApi.Responses;

namespace InternalApi.Interfaces;

public interface ICurrencyHttpApi
{
    Task<QuotaResponse> GetApiQuotasAsync();

    Task<CurrencyResponse> GetAllCurrenciesRateAsync(string baseCurrency, CancellationToken cancellationToken);

    Task<CurrencyResponse> GetAllCurrenciesDataWithRateAsync(
        string currencyCode,
        DateOnly date,
        CancellationToken cancellationToken);
}