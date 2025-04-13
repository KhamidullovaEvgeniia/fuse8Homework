using Fuse8.BackendInternship.PublicApi.Models;
using Google.Protobuf.WellKnownTypes;

namespace Fuse8.BackendInternship.PublicApi.Interfaces;

public interface ICurrencyApiService
{
    Task<CurrencyRate> GetCurrencyRateAsync(string currencyCode, CancellationToken cancellationToken);

    Task<DatedCurrencyRate> GetCurrencyDataWithRateAsync(string currencyCode, DateOnly date, CancellationToken cancellationToken);

    Task<ApiSettings> GetApiSettingsAsync(Empty request);
}