using Capstone.Core.Storage;
using Capstone.Core.ViewModels;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab09Wiring : ILab09Wiring
{
    public IItemStore CreateStore() => new Library();

    public IItem CreateSample(int seed) => SampleTracks.Create(seed);

    public ICatalogueViewModel CreateViewModel(IItemStore store) => new CatalogueViewModel((Library)store);
}
