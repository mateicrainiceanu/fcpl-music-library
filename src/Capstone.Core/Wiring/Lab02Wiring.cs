using Capstone.Core.Json;
using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab02Wiring : ILab02Wiring
{
    public IItemStore CreateStore() => new Library();

    public IItemMapper CreateMapper() => new TrackMapper();

    public IItem CreateSample(int seed) => SampleTracks.Create(seed);
}
