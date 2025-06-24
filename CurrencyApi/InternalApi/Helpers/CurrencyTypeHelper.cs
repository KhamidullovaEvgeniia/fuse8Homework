using Currency;
using General.Enums;

namespace InternalApi.Helpers;

public static class CurrencyTypeHelper
{
    public static CurrencyType ParsingCurrencyCode(CurrencyCode inputCurrencyCode)
    {
        var currencyCode = (CurrencyType)inputCurrencyCode;
        if (currencyCode == default || Enum.IsDefined(currencyCode) is false)
        {
            throw new InvalidOperationException($"В запросе пришел неизвестный код валюты: '{inputCurrencyCode}'");
        }

        return currencyCode;
    }
}