using System.Text;

namespace Fuse8.BackendInternship.Domain;

public static class DomainExtensions
{
	// ToDo: реализовать экстеншены

	public static bool IsNullOrEmpty<T>(this IEnumerable<T>? items)
	{
		return items == null || !items.Any();
	}

	public static string JoinToString<T>(this IEnumerable<T>? items, string separator)
	{
		return items is not null ? string.Join(separator, items) : string.Empty;
	}

	public static int DaysCountBetween(this DateTimeOffset firstDateTimeOffset, DateTimeOffset secondDateTimeOffset)
	{
		return Math.Abs((firstDateTimeOffset.Date - secondDateTimeOffset.Date).Days);
	}
}