using Currency;
using General.Enums;
using General.Helper;

namespace Fuse8.BackendInternship.PublicApi.Helpers;

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
    
    public static CurrencyCode ToCurrencyCode(CurrencyType currencyCodeFromRequest)
    {
        var currencyCode = (CurrencyCode)currencyCodeFromRequest;
        if (currencyCode == default || Enum.IsDefined(currencyCode) is false)
        {
            throw new InvalidOperationException($"В запросе пришел неизвестный код валюты: '{currencyCodeFromRequest}'");
        }
        return currencyCode;
    }
    
    public static CurrencyCode ToCurrencyCode(string currency)
    {
        var currencyType = CurrencyHelper.ParsingCurrencyCode(currency);
        var currencyCode = ToCurrencyCode(currencyType);
        return currencyCode;
    }
    
    public static string ParsingCurrencyCodeToString(CurrencyCode currencyCode)
    {
        var resultType = ParsingCurrencyCode(currencyCode);
        var resultCode = CurrencyHelper.ToCurrencyCode(resultType);
        return resultCode;
    }
}