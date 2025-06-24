namespace InternalApi.DataAccess.Models;

public sealed class ExchangeDate
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public ICollection<CurrencyRate> CurrencyRates { get; set; }
}