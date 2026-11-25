using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab08Wiring : ILab08Wiring
{
    private readonly Lab04Wiring _kinds = new();

    public IItemStore CreateStore() => new Library();

    public IItem CreateSample(int seed) => SampleTracks.Create(seed);

    public IReadOnlyList<Type> KindTypes => _kinds.KindTypes;

    public IItem CreateOfKind(int kindIndex, int seed) => _kinds.CreateOfKind(kindIndex, seed);

    public Task SaveAsync(IItemStore store, string path) => LibraryFile.SaveAsync(store, path);

    public async Task<IItemStore> LoadAsync(string path) => await LibraryFile.LoadAsync(path);
}
