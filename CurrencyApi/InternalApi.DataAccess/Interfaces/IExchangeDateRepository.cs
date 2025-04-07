using InternalApi.DataAccess.Models;

namespace InternalApi.DataAccess.Interfaces;

public interface IExchangeDateRepository
{
    Task<ExchangeDate?> FindByDateWithinExpirationAsync(DateTime targetDate, TimeSpan expiration);
    Task<ExchangeDate?> AddAsync(DateTime date);
    Task SaveChangesAsync();
}