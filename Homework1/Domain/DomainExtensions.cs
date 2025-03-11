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
		if (items.IsNullOrEmpty())
			return string.Empty;
		
		StringBuilder stringBuilder = new StringBuilder(items.Count() * 2);
		foreach (var item in items)
		{
			if (stringBuilder.Length > 0)
				stringBuilder.Append(separator);
			
			stringBuilder.Append(item);
		}

		return stringBuilder.ToString();
	}

	public static int DaysCountBetween(this DateTimeOffset firstDateTimeOffset, DateTimeOffset secondDateTimeOffset)
	{
		return (firstDateTimeOffset.Date - secondDateTimeOffset.Date).Days;
	}
}