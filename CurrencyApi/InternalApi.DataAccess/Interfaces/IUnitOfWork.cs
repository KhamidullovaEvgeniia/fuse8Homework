namespace InternalApi.DataAccess.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICurrencyRateRepository CurrencyRateRepository { get; }
    IExchangeDateRepository ExchangeDateRepository { get; }
    
    void Commit();
}