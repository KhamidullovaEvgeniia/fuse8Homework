using Currency;
using Framework.Enums;
using Fuse8.BackendInternship.PublicApi.Models;
using Google.Protobuf.WellKnownTypes;

namespace Fuse8.BackendInternship.PublicApi.Interfaces;

public interface ICurrencyApiService
{
    Task<CurrencyRate> GetCurrencyRateAsync(CurrencyType currencyCode, CancellationToken cancellationToken);

    Task<DatedCurrencyRate> GetCurrencyDataWithRateAsync(CurrencyType currencyCode, DateOnly date, CancellationToken cancellationToken);

    Task<ApiSettings> GetApiSettingsAsync(Empty request, CancellationToken cancellationToken);
}