using System.Reflection;

namespace Fuse8.BackendInternship.Domain;

public static class BankCardHelpers
{
    /// <summary>
    /// Получает номер карты без маски
    /// </summary>
    /// <param name="card">Банковская карта</param>
    /// <returns>Номер карты без маски</returns>
    public static string GetUnmaskedCardNumber(BankCard card)
    {
        // ToDo: С помощью рефлексии получить номер карты без маски

        if (card is null)
            throw new ArgumentNullException(nameof(card));

        const string NumberFieldName = "_number";
        var type = card.GetType();
        var fieldInfo = type.GetField(NumberFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo == null)
            throw new InvalidOperationException(
                $"Не получилось достать номер без маски: в классе {nameof(BankCard)} нет поля '{NumberFieldName}'");

        var notMaskedNumberObj = fieldInfo.GetValue(card);
        if (notMaskedNumberObj is null)
        {
            throw new InvalidOperationException(
                $"Не получилось достать номер карты без маски: в  в классе {nameof(BankCard)} поле '{NumberFieldName}' = null");
        }

        if (notMaskedNumberObj is not string notMaskedNumber)
        {
            throw new InvalidOperationException(
                $"Не получилось достать номер карты без маски: в классе {nameof(BankCard)} поле '{NumberFieldName}' имеет неправильный тип '{notMaskedNumberObj.GetType()}'");
        }

        return notMaskedNumber;
    }
}