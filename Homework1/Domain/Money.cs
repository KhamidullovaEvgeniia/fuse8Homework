namespace Fuse8.BackendInternship.Domain;

/// <summary>
/// Модель для хранения денег
/// </summary>
public class Money
{
    public Money(int rubles, int kopeks) : this(false, rubles, kopeks)
    {
    }

    public Money(bool isNegative, int rubles, int kopeks)
    {
        if ((isNegative && rubles == 0 && kopeks == 0) || rubles < 0 || kopeks > 99 || kopeks < 0)
            throw new ArgumentException();

        IsNegative = isNegative;
        Rubles = rubles;
        Kopeks = kopeks;
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
        var firstTotalKopeks = GetTotalKopeks(first);
        var secondTotalKopeks = GetTotalKopeks(second);

        bool isNegative = first.IsNegative;
        int sum;

        if (first.IsNegative == second.IsNegative)
        {
            sum = firstTotalKopeks + secondTotalKopeks;
        }
        else
        {
            sum = Math.Abs(firstTotalKopeks - secondTotalKopeks);

            isNegative = firstTotalKopeks < secondTotalKopeks ? !first.IsNegative : first.IsNegative;
        }

        return new Money(isNegative, sum / 100, sum % 100);
    }

    public static Money operator -(Money first, Money second)
    {
        var firstTotalKopeks = GetTotalKopeks(first);
        var secondTotalKopeks = GetTotalKopeks(second);

        var sum = Math.Abs(firstTotalKopeks - secondTotalKopeks);

        bool isNegative = firstTotalKopeks < secondTotalKopeks ? !first.IsNegative : first.IsNegative;

        return new Money(isNegative, sum / 100, sum % 100);
    }

    public static bool operator >(Money first, Money second)
    {
        if (first.IsNegative != second.IsNegative)
            return !first.IsNegative;

        var firstTotalKopeks = GetTotalKopeks(first);
        var secondTotalKopeks = GetTotalKopeks(second);

        return first.IsNegative ? firstTotalKopeks < secondTotalKopeks : firstTotalKopeks > secondTotalKopeks;
    }

    public static bool operator >=(Money first, Money second)
    {
        if (first.IsNegative != second.IsNegative)
            return !first.IsNegative;

        var firstTotalKopeks = GetTotalKopeks(first);
        var secondTotalKopeks = GetTotalKopeks(second);

        return first.IsNegative ? firstTotalKopeks <= secondTotalKopeks : firstTotalKopeks >= secondTotalKopeks;
    }

    public static bool operator <(Money first, Money second)
    {
        if (first.IsNegative != second.IsNegative)
            return first.IsNegative;

        var firstTotalKopeks = GetTotalKopeks(first);
        var secondTotalKopeks = GetTotalKopeks(second);

        return first.IsNegative ? firstTotalKopeks > secondTotalKopeks : firstTotalKopeks < secondTotalKopeks;
    }

    public static bool operator <=(Money first, Money second)
    {
        if (first.IsNegative != second.IsNegative)
            return first.IsNegative;

        var firstTotalKopeks = GetTotalKopeks(first);
        var secondTotalKopeks = GetTotalKopeks(second);

        return first.IsNegative ? firstTotalKopeks >= secondTotalKopeks : firstTotalKopeks <= secondTotalKopeks;
    }

    public override string ToString() => $"({IsNegative}, {Rubles}, {Kopeks})";

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        return Equals((Money)obj);
    }

    private bool Equals(Money other)
    {
        return IsNegative == other.IsNegative && Rubles == other.Rubles && Kopeks == other.Kopeks;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsNegative, Rubles, Kopeks);
    }

    private static int GetTotalKopeks(Money money) => money.Rubles * 100 + money.Kopeks;
}