using Fuse8.BackendInternship.PublicApi.Responses;

namespace Fuse8.BackendInternship.PublicApi.Interfaces;

public interface ICurrencyHttpApi
{
    Task<CurrencyResponse> GetCurrencyRateAsync(string currencyCode);

    Task<CurrencyResponse> GetCurrencyDataWithRateAsync(string currencyCode, DateTime date);

    Task<QuotaResponse> GetApiQuotasAsync();
}