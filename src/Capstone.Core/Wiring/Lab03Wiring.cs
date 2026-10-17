using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab03Wiring : ILab03Wiring
{
    public IItemStore CreateStore() => new Library();

    public IItem CreateSample(int seed) => SampleTracks.Create(seed);

    public IReadOnlyList<IItem> Search(IItemStore store, string query)
    {
        var matcher = SearchMatcher.Create();
        return store.All()
            .Where(item => matcher.Matches(item.DisplayName, query))
            .ToList();
    }
}
