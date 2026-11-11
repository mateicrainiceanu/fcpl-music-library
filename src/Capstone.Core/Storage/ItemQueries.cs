using SharpQuest.Contracts;

namespace Capstone.Core.Storage;

public static class ItemQueries
{
    public static IEnumerable<IItem> Longest(this IEnumerable<IItem> items, int n)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        return items
            .OrderByDescending(i => i.DisplayName.Length)
            .ThenBy(i => i.Id, StringComparer.Ordinal)
            .Take(n);
    }

    /// <summary>By display name ignoring case, ties broken by Id.</summary>
    public static IOrderedEnumerable<IItem> InNameOrder(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items
            .OrderBy(i => i.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(i => i.Id, StringComparer.Ordinal);
    }

    /// <summary>One page of name order. Negative skip/take is a programming error.</summary>
    public static IEnumerable<IItem> Page(this IEnumerable<IItem> items, int skip, int take)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegative(take);

        return items.InNameOrder().Skip(skip).Take(take);
    }

    /// <summary>The whole sequence in one line.</summary>
    public static StoreStats Summary(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var list = items.ToList();
        if (!list.Any()) return new StoreStats(0, 0, null, 0);

        return new StoreStats(
            Count: list.Count(),
            DistinctInitials: list
                .Select(i => i.DisplayName)
                .Where(n => n.Length > 0)
                .Select(n => char.ToUpperInvariant(n[0]))
                .Distinct()
                .Count(),
            LongestName: list.Longest(1).First().DisplayName,
            AverageNameLength: list.Average(i => i.DisplayName.Length));
    }
}
