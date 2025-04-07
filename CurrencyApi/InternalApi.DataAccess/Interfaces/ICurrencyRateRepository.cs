using InternalApi.DataAccess.Models;

namespace InternalApi.DataAccess.Interfaces;

public interface ICurrencyRateRepository
{
    Task<List<CurrencyRate>> GetAllAsync();
    Task<CurrencyRate> GetByKeyAsync(int currency, int dateId);
    Task AddAsync(CurrencyRate currencyRate);
    Task SaveChangesAsync();
}