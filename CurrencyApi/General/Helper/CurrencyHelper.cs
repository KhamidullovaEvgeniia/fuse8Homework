using Framework.Enums;

namespace Framework.Helper;

public static class CurrencyHelper
{
    public static CurrencyType ParsingCurrencyCode(string currencyCode)
    {
        return currencyCode switch
        {
            "USD" => CurrencyType.USD,
            "RUB" => CurrencyType.RUB,
            "KZT" => CurrencyType.KZT,
            _ => throw new InvalidOperationException($"Invalid currency code: '{currencyCode}'")
        };
    }

    public static string ToCurrencyCode(CurrencyType currencyCode)
    {
        return currencyCode switch
        {
            CurrencyType.USD => "USD",
            CurrencyType.RUB => "RUB",
            CurrencyType.KZT => "KZT",
            _ => throw new InvalidOperationException($"Invalid currency code: '{currencyCode}'")
        };
    }
}