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

        if (card == null)
            throw new ArgumentNullException(nameof(card));

        var type = card.GetType();
        var fieldInfo = type.GetField("_number", BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo == null)
            throw new ArgumentNullException(message: "Поле не должно быть пустым", paramName: nameof(fieldInfo));

        return fieldInfo.GetValue(card)?.ToString()
               ?? throw new ArgumentNullException(
                   message: "Значение не должно быть пустым",
                   paramName: nameof(fieldInfo.GetValue));
    }
}