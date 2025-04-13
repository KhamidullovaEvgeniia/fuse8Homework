using PublicApi.DataAccess.Models;

namespace PublicApi.DataAccess.Interfaces;

public interface IFavoriteCurrencyRateRepository
{
    Task<IEnumerable<FavoriteCurrencyRate>> GetAllAsync();
    Task<FavoriteCurrencyRate?> GetByNameAsync(string name);
    Task AddAsync(FavoriteCurrencyRate rate);
    Task UpdateByNameAsync(FavoriteCurrencyRate rate);
    Task DeleteAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
    Task<bool> ExistsByCurrenciesAsync(string currency, string baseCurrency);
}