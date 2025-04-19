using InternalApi.DataAccess.Interfaces;
using InternalApi.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace InternalApi.DataAccess.Repositories;

public class CurrencyRateRepository : ICurrencyRateRepository
{
    private readonly CurrencyDbContext _context;

    public CurrencyRateRepository(CurrencyDbContext context)
    {
        _context = context;
    }

    public async Task<CurrencyRate?> GetByKeyAsync(int currency, int dateId, CancellationToken cancellationToken)
    {
        return await _context.CurrencyRates.FirstOrDefaultAsync(
            c => c.Currency == currency && c.DateId == dateId,
            cancellationToken);
    }

    public async Task<List<CurrencyRate>> GetByCurrencyTypesAndDateAsync(
        IEnumerable<int> currencyTypes,
        int exchangeDateId,
        CancellationToken cancellationToken)
    {
        return await _context
            .CurrencyRates
            .Where(rate => currencyTypes.Contains(rate.Currency) && rate.DateId == exchangeDateId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CurrencyRate currencyRate, CancellationToken cancellationToken)
    {
        await _context.CurrencyRates.AddAsync(currencyRate, cancellationToken);
    }
}