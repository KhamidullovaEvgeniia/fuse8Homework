using Fuse8.BackendInternship.PublicApi.Models;

namespace Fuse8.BackendInternship.PublicApi.Interfaces;

public interface ICurrencyApiService
{
    Task<CurrencyRate> GetCurrencyRateAsync(string currencyCode);

    Task<DatedCurrencyRate> GetCurrencyDataWithRateAsync(string currencyCode, DateTime date);

    Task<ApiSettings> GetApiSettingsAsync();
}