namespace InternalApi.DataAccess.Models;

public class CurrencyRate
{
    public int Currency { get; set; }
    public decimal Value { get; set; }

    public int DateId { get; set; }
    public required ExchangeDate ExchangeDate { get; set; }
}