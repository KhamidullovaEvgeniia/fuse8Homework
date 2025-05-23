﻿namespace Fuse8.BackendInternship.Domain;

/// <summary>
/// Значения ресурсов для календаря
/// </summary>
public class CalendarResource
{
    public static readonly CalendarResource Instance = new();

    private static readonly string[] MonthNames =
    {
        "Январь",
        "Февраль",
        "Март",
        "Апрель",
        "Май",
        "Июнь",
        "Июль",
        "Август",
        "Сентябрь",
        "Октябрь",
        "Ноябрь",
        "Декабрь",
    };

    public static readonly string January = GetMonthByNumber(0);

    public static readonly string February = GetMonthByNumber(1);

    private static string GetMonthByNumber(int number) => MonthNames[number];

    // ToDo: реализовать индексатор для получения названия месяца по енаму Month

    public string this[Month month]
    {
        get
        {
            int index = (int)month;
            if (index < 0 || index >= MonthNames.Length)
                throw new ArgumentOutOfRangeException(nameof(month), "");

            return MonthNames[index];
        }
    }
}

public enum Month
{
    January,

    February,

    March,

    April,

    May,

    June,

    July,

    August,

    September,

    October,

    November,

    December,
}