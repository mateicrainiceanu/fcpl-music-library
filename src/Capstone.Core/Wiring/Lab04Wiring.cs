using Capstone.Core.Model;
using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab04Wiring : ILab04Wiring
{
    public IItemStore CreateStore() => new Library();

    public Type BaseType => typeof(LibraryItem);

    public IReadOnlyList<Type> KindTypes { get; } = [typeof(Track), typeof(Album)];

    public IItem CreateOfKind(int kindIndex, int seed) => kindIndex switch
    {
        0 => SampleTracks.Create(seed),
        1 => new Album
        {
            Id = $"alb-{seed:D3}",
            Title = $"Album {seed}",
            Artist = $"Artist {seed}",
            ReleaseYear = 1970 + Math.Abs(seed) % 50,
        },
        _ => throw new ArgumentOutOfRangeException(nameof(kindIndex), kindIndex, "There are two kinds: 0 and 1."),
    };

    public string Describe(IItem item) => ((LibraryItem)item).Describe();

    public IReadOnlyList<string> DescribeAll(IItemStore store)
    {
        var descriptions = new List<string>();
        foreach (var item in store.All())
            descriptions.Add(((LibraryItem)item).Describe());
        return descriptions;
    }

    public string Classify(IItem item) => item switch
    {
        Track => "Track",
        Album => "Album",
        null => throw new ArgumentNullException(nameof(item)),
        _ => "Unknown",
    };

    public bool TryFindByName(IItemStore store, string name, out IItem? found)
    {
        found = string.IsNullOrWhiteSpace(name)
            ? null
            : store.All().FirstOrDefault(i => i.DisplayName == name);
        return found is not null;
    }

    public int AddAll(IItemStore store, params IItem[] items)
    {
        var added = 0;
        foreach (var item in items)
        {
            store.Add(item);
            added++;
        }
        return added;
    }
}
