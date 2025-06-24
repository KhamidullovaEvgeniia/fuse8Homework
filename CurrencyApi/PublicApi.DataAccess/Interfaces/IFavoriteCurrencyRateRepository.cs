using PublicApi.DataAccess.Models;

namespace PublicApi.DataAccess.Interfaces;

public interface IFavoriteCurrencyRateRepository
{
    Task<IEnumerable<FavoriteCurrencyRate>> GetAllAsync(CancellationToken cancellationToken);
    Task<FavoriteCurrencyRate?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task AddAsync(FavoriteCurrencyRate rate, CancellationToken cancellationToken);
    Task UpdateByNameAsync(FavoriteCurrencyRate rate, CancellationToken cancellationToken);
    Task DeleteAsync(string name, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
    Task<bool> ExistsByCurrenciesAsync(string currency, string baseCurrency, CancellationToken cancellationToken);
}