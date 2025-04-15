namespace InternalApi.DataAccess.Models;

public class ExchangeDate
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public ICollection<CurrencyRate> CurrencyRates { get; set; }
}