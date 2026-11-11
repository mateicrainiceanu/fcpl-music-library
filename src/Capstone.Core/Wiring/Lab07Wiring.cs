using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab07Wiring : ILab07Wiring
{
    public IItemStore CreateStore() => new Library();

    public IItem CreateSample(int seed) => SampleTracks.Create(seed);

    // Deferred: queries the library itself, so later adds/removes are seen on enumeration.
    public IEnumerable<string> Names(IItemStore store) => ((Library)store).Select(i => i.DisplayName);

    public IReadOnlyList<IItem> Sorted(IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return InNameOrder(items).ToList();
    }

    public IReadOnlyList<IItem> Page(IEnumerable<IItem> items, int skip, int take)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegative(take);
        return InNameOrder(items).Skip(skip).Take(take).ToList();
    }

    public IReadOnlyList<IItem> Longest(IEnumerable<IItem> items, int n) => items.Longest(n).ToList();

    public Type ExtensionHost => typeof(ItemQueries);

    public StoreStats Summary(IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var list = items.ToList();
        if (!list.Any()) return new StoreStats(0, 0, null, 0);

        var names = list.Select(i => i.DisplayName).ToList();
        return new StoreStats(
            Count: names.Count(),
            DistinctInitials: names.Where(n => n.Length > 0).Select(n => char.ToUpperInvariant(n[0])).Distinct().Count(),
            LongestName: list.Longest(1).First().DisplayName,
            AverageNameLength: names.Average(n => n.Length));
    }

    private static IOrderedEnumerable<IItem> InNameOrder(IEnumerable<IItem> items) =>
        items
            .OrderBy(i => i.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(i => i.Id, StringComparer.Ordinal);
}
