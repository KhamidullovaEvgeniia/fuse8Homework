using Fuse8.BackendInternship.PublicApi.Models;

namespace Fuse8.BackendInternship.PublicApi.Interfaces;

public interface IFavoriteCurrencyService
{
    Task<FavoriteCurrencyRateDTO> GetFavoriteCurrencyRateByNameAsync(string name, CancellationToken cancellationToken);

    Task<FavoriteCurrencyRateDTO[]> GetAllFavoriteCurrencyRatesAsync(CancellationToken cancellationToken);

    Task AddFavoriteCurrencyRateAsync(FavoriteCurrencyRateDTO currencyRateDTO, CancellationToken cancellationToken);

    Task UpdateFavoriteCurrencyRateAsync(FavoriteCurrencyRateDTO rateDTO, CancellationToken cancellationToken);

    Task DeleteFavoriteCurrencyRateByNameAsync(string name, CancellationToken cancellationToken);

    Task<CurrencyRate> GetSelectedCurrencyRateByName(string name, CancellationToken cancellationToken);
    
    Task<DatedCurrencyRate> GetSelectedCurrencyRateByDate(string name, DateOnly date, CancellationToken cancellationToken);
}