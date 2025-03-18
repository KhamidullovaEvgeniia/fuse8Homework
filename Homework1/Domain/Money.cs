namespace Fuse8.BackendInternship.Domain;

/// <summary>
/// Модель для хранения денег
/// </summary>
public class Money : IComparable<Money>
{
    /// <summary>
    /// Количество копеек в 1 рубле
    /// </summary>
    private const int KopeksFactor = 100;

    private readonly long _totalKopeks;

    public Money(int rubles, int kopeks) : this(false, rubles, kopeks)
    {
    }

    public Money(bool isNegative, int rubles, int kopeks)
    {
        if (kopeks > 99)
            throw new ArgumentException(message: "Копейки не должны быть больше 99", paramName: nameof(kopeks));

        if (kopeks < 0)
            throw new ArgumentException(message: "Копейки не должны быть меньше 0", paramName: nameof(kopeks));

        if (rubles < 0)
            throw new ArgumentException(message: "Рубли не должны быть меньше 0", paramName: nameof(rubles));

        if (isNegative && rubles == 0 && kopeks == 0)
            throw new ArgumentException(message: "Нулевое значение не может быть отрицательным", paramName: nameof(isNegative));

        IsNegative = isNegative;
        Rubles = rubles;
        Kopeks = kopeks;

        var totalKopeks = (long)rubles * KopeksFactor + kopeks;
        _totalKopeks = isNegative ? totalKopeks * -1 : totalKopeks;
    }

    /// <summary>
    /// Отрицательное значение
    /// </summary>
    public bool IsNegative { get; }

    /// <summary>
    /// Число рублей
    /// </summary>
    public int Rubles { get; }

    /// <summary>
    /// Количество копеек
    /// </summary>
    public int Kopeks { get; }

    public static Money operator +(Money first, Money second)
    {
        var sumKopeks = first._totalKopeks + second._totalKopeks;
        return CreateFromTotalKopeks(sumKopeks);
    }

    public static Money operator -(Money first, Money second)
    {
        var diffKopeks = first._totalKopeks - second._totalKopeks;
        return CreateFromTotalKopeks(diffKopeks);
    }

    public int CompareTo(Money? other)
    {
        if (ReferenceEquals(this, other))
            return 0;

        if (other is null)
            return 1;

        return _totalKopeks.CompareTo(other._totalKopeks);
    }

    public static bool operator ==(Money? first, Money? second) => first?.CompareTo(second) == 0;

    public static bool operator !=(Money? first, Money? second) => first?.CompareTo(second) != 0;

    public static bool operator >(Money? first, Money? second) => first?.CompareTo(second) > 0;

    public static bool operator >=(Money? first, Money? second) => first?.CompareTo(second) >= 0;

    public static bool operator <(Money? first, Money? second) => first?.CompareTo(second) < 0;

    public static bool operator <=(Money? first, Money? second) => first?.CompareTo(second) <= 0;

    public override bool Equals(object? obj) => obj is Money other && this.CompareTo(other) == 0;

    public override string ToString() => $"({(IsNegative ? '-' : string.Empty)}{Rubles},{Kopeks} \u20bd)";

    public override int GetHashCode()
    {
        return HashCode.Combine(IsNegative, Rubles, Kopeks);
    }

    private static Money CreateFromTotalKopeks(long totalKopeks)
    {
        var absTotalKopeks = Math.Abs(totalKopeks);
        return new Money(
            isNegative: totalKopeks < 0,
            rubles: (int)(absTotalKopeks / KopeksFactor),
            kopeks: (int)(absTotalKopeks % KopeksFactor));
    }
}