using Capstone.Core.Model;
using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab05Wiring : ILab05Wiring
{
    public IItemStore CreateStore() => new Library();

    public IItem CreateSample(int seed) => SampleTracks.Create(seed);

    public Type CollectionType => typeof(Library);

    public Type GenericDefinition => typeof(RecentlyUsed<>);

    public IItem At(IItemStore store, int index) => ((Library)store)[index];

    public IItem? ById(IItemStore store, string id) => ((Library)store)[id];

    public IReadOnlyList<IItem> EachInOrder(IItemStore store)
    {
        var items = new List<IItem>();
        foreach (var item in (Library)store)
            items.Add(item);
        return items;
    }

    public IEnumerable<IItem> Matching(IItemStore store, string text) => ((Library)store).Matching(text);

    public IReadOnlyList<T> Recent<T>(int capacity, params T[] pushed)
    {
        var recent = new RecentlyUsed<T>(capacity);
        foreach (var value in pushed)
            recent.Push(value);
        return recent.Items;
    }
}
