namespace InternalApi.DataAccess.Models;

public class CurrencyRate
{
    public int Currency { get; set; }
    public decimal Value { get; set; }

    public int DateId { get; set; }
    public ExchangeDate ExchangeDate { get; set; }
}