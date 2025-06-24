using InternalApi.DataAccess.Models;

namespace InternalApi.DataAccess.Interfaces;

public interface IExchangeDateRepository
{
    Task<ExchangeDate?> FindByDateWithinExpirationAsync(
        DateTime targetDate,
        TimeSpan expiration,
        CancellationToken cancellationToken);

    Task<ExchangeDate?> AddAsync(DateTime date, CancellationToken cancellationToken);
}