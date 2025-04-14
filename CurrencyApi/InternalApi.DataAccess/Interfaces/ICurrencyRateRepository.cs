using InternalApi.DataAccess.Models;

namespace InternalApi.DataAccess.Interfaces;

public interface ICurrencyRateRepository
{
    Task<CurrencyRate?> GetByKeyAsync(int currency, int dateId, CancellationToken cancellationToken);

    Task<List<CurrencyRate>> GetByCurrencyTypesAndDateAsync(IEnumerable<int> currencyTypes, int exchangeDateId, CancellationToken cancellationToken);

    Task AddAsync(CurrencyRate currencyRate, CancellationToken cancellationToken);
}