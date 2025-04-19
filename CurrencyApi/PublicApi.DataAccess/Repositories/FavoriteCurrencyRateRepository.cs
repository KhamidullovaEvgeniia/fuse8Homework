using Microsoft.EntityFrameworkCore;
using PublicApi.DataAccess.Interfaces;
using PublicApi.DataAccess.Models;

namespace PublicApi.DataAccess.Repositories;

public class FavoriteCurrencyRateRepository : IFavoriteCurrencyRateRepository
{
    private readonly FavoriteCurrencyDbContext _context;

    public FavoriteCurrencyRateRepository(FavoriteCurrencyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FavoriteCurrencyRate>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.FavoriteCurrencyRates.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<FavoriteCurrencyRate?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _context.FavoriteCurrencyRates.FirstOrDefaultAsync(r => r.Name == name, cancellationToken: cancellationToken);
    }

    public async Task AddAsync(FavoriteCurrencyRate rate, CancellationToken cancellationToken)
    {
        await _context.FavoriteCurrencyRates.AddAsync(rate, cancellationToken: cancellationToken);
        await _context.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    public async Task UpdateByNameAsync(FavoriteCurrencyRate rate, CancellationToken cancellationToken)
    {
        var entity = await _context.FavoriteCurrencyRates.FirstOrDefaultAsync(c => c.Name == rate.Name, cancellationToken: cancellationToken);

        if (entity == null)
            throw new InvalidOperationException("Favorite rate not found");

        entity.Currency = rate.Currency;
        entity.BaseCurrency = rate.BaseCurrency;

        await _context.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string name, CancellationToken cancellationToken)
    {
        var entity = await _context.FavoriteCurrencyRates.FirstOrDefaultAsync(x => x.Name == name, cancellationToken: cancellationToken);
        if (entity != null)
        {
            _context.FavoriteCurrencyRates.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken: cancellationToken);
        }
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _context.FavoriteCurrencyRates.AnyAsync(r => r.Name == name, 
            cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsByCurrenciesAsync(string currency, string baseCurrency, CancellationToken cancellationToken)
    {
        return await _context.FavoriteCurrencyRates.AnyAsync(r => r.Currency == currency && r.BaseCurrency == baseCurrency, 
            cancellationToken: cancellationToken);
    }
}