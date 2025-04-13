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

    public async Task<IEnumerable<FavoriteCurrencyRate>> GetAllAsync()
    {
        return await _context.FavoriteCurrencyRates.ToListAsync();
    }

    public async Task<FavoriteCurrencyRate?> GetByNameAsync(string name)
    {
        return await _context.FavoriteCurrencyRates.FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task AddAsync(FavoriteCurrencyRate rate)
    {
        _context.FavoriteCurrencyRates.Add(rate);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateByNameAsync(FavoriteCurrencyRate rate)
    {
        var entity = await _context.FavoriteCurrencyRates.FirstOrDefaultAsync(c => c.Name == rate.Name);

        if (entity == null)
            throw new InvalidOperationException("Favorite rate not found");

        entity.Currency = rate.Currency;
        entity.BaseCurrency = rate.BaseCurrency;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string name)
    {
        var entity = await _context.FavoriteCurrencyRates.FirstOrDefaultAsync(x => x.Name == name);
        if (entity != null)
        {
            _context.FavoriteCurrencyRates.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.FavoriteCurrencyRates.AnyAsync(r => r.Name == name);
    }

    public async Task<bool> ExistsByCurrenciesAsync(string currency, string baseCurrency)
    {
        return await _context.FavoriteCurrencyRates.AnyAsync(r => r.Currency == currency && r.BaseCurrency == baseCurrency);
    }
}