using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab06Wiring : ILab06Wiring
{
    public IItemStore CreateStore() => new Library();

    public IItem CreateSample(int seed) => SampleTracks.Create(seed);

    public Type PublisherType => typeof(Library);

    public Action Subscribe(IItemStore store, Action<IItem> handler)
    {
        var library = (Library)store;
        library.ItemAdded += handler;
        return () => library.ItemAdded -= handler;
    }

    public IReadOnlyList<IItem> Where(IItemStore store, Func<IItem, bool> keep) => ((Library)store).Where(keep);

    public IReadOnlyList<string> Map(IItemStore store, Func<IItem, string> describe) => ((Library)store).Map(describe);

    public void ForEach(IItemStore store, Action<IItem> action) => ((Library)store).ForEach(action);
}
