using InternalApi.DataAccess.Models;

namespace InternalApi.DataAccess.Interfaces;

public interface ICurrencyRateRepository
{
    Task<CurrencyRate?> GetByKeyAsync(int currency, int dateId);

    Task<List<CurrencyRate>> GetByCurrencyTypesAndDateAsync(IEnumerable<int> currencyTypes, int exchangeDateId);

    Task AddAsync(CurrencyRate currencyRate);

    Task SaveChangesAsync();
}