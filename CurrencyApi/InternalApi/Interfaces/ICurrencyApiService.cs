

using InternalApi.Models;

namespace InternalApi.Interfaces;

public interface ICurrencyApiService
{
    Task<CurrencyRates> GetCurrencyRateAsync(string currencyCode);

    Task<DatedCurrencyRate> GetCurrencyDataWithRateAsync(string currencyCode, DateOnly date);

    Task<ApiSettings> GetApiSettingsAsync();
}