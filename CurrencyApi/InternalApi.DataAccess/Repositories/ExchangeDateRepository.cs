using InternalApi.DataAccess.Interfaces;
using InternalApi.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace InternalApi.DataAccess.Repositories;

public class ExchangeDateRepository : IExchangeDateRepository
{
    private readonly CurrencyDbContext _context;

    public ExchangeDateRepository(CurrencyDbContext context)
    {
        _context = context;
    }

    public async Task<ExchangeDate?> FindByDateWithinExpirationAsync(
        DateTime targetDate,
        TimeSpan expiration,
        CancellationToken cancellationToken)
    {
        var minDate = targetDate - expiration;

        return await _context
            .ExchangeDates
            .Include(e => e.CurrencyRates)
            .FirstOrDefaultAsync(e => e.Date >= minDate && e.Date <= targetDate, cancellationToken);
    }

    public async Task<ExchangeDate?> AddAsync(DateTime date, CancellationToken cancellationToken)
    {
        var entity = new ExchangeDate { Date = date };
        await _context.ExchangeDates.AddAsync(entity, cancellationToken);
        return entity;
    }
}