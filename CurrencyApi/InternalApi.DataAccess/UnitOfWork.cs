using InternalApi.DataAccess.Interfaces;
using InternalApi.DataAccess.Repositories;

namespace InternalApi.DataAccess;

public class UnitOfWork : IUnitOfWork
{
    private readonly CurrencyDbContext _context;
    private readonly IExchangeDateRepository _exchangeDateRepository;
    private readonly ICurrencyRateRepository _currencyRateRepository;

    public UnitOfWork(CurrencyDbContext context)
    {
        _context = context;
        _exchangeDateRepository = new ExchangeDateRepository(_context);
        _currencyRateRepository = new CurrencyRateRepository(_context);
    }

    public IExchangeDateRepository ExchangeDateRepository => _exchangeDateRepository;

    public ICurrencyRateRepository CurrencyRateRepository => _currencyRateRepository;

    public void Commit()
    {
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}