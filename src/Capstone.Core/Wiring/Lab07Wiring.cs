using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab07Wiring : ILab07Wiring
{
    public IItemStore CreateStore() => new Library();

    public IItem CreateSample(int seed) => SampleTracks.Create(seed);

    // Deferred: queries the library itself, so later adds/removes are seen on enumeration.
    public IEnumerable<string> Names(IItemStore store) => ((Library)store).Select(i => i.DisplayName);

    public IReadOnlyList<IItem> Sorted(IEnumerable<IItem> items) => items.InNameOrder().ToList();

    public IReadOnlyList<IItem> Page(IEnumerable<IItem> items, int skip, int take) => items.Page(skip, take).ToList();

    public IReadOnlyList<IItem> Longest(IEnumerable<IItem> items, int n) => items.Longest(n).ToList();

    public Type ExtensionHost => typeof(ItemQueries);

    public StoreStats Summary(IEnumerable<IItem> items) => items.Summary();
}
