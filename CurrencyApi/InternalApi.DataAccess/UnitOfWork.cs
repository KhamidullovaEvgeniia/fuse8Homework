using InternalApi.DataAccess.Interfaces;
using InternalApi.DataAccess.Repositories;

namespace InternalApi.DataAccess;

public class UnitOfWork : IUnitOfWork
{
    private readonly CurrencyDbContext _context;
    private IExchangeDateRepository _exchangeDateRepository;
    private ICurrencyRateRepository _currencyRateRepository;

    public UnitOfWork(CurrencyDbContext context)
    {
        _context = context;
    }

    public IExchangeDateRepository ExchangeDateRepository
    {
        get { return _exchangeDateRepository ??= new ExchangeDateRepository(_context); }
    }

    public ICurrencyRateRepository CurrencyRateRepository
    {
        get { return _currencyRateRepository ??= new CurrencyRateRepository(_context); }
    }

    public void Commit()
    {
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}