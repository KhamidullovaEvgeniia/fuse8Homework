using InternalApi.DataAccess.Interfaces;
using InternalApi.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace InternalApi.DataAccess.Repositories;

public class CurrencyRateRepository: ICurrencyRateRepository
{
    private readonly CurrencyDbContext _context;

    public CurrencyRateRepository(CurrencyDbContext context)
    {
        _context = context;
    }

    public async Task<List<CurrencyRate>> GetAllAsync()
    {
        return await _context.CurrencyRates.ToListAsync();
    }

    public async Task<CurrencyRate?> GetByKeyAsync(int currency, int dateId)
    {
        return await _context.CurrencyRates
            .FirstOrDefaultAsync(c => c.Currency == currency && c.DateId == dateId);
    }

    public async Task AddAsync(CurrencyRate currencyRate)
    {
        await _context.CurrencyRates.AddAsync(currencyRate);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}