using SharpQuest.Contracts;

namespace Capstone.Core.Model;

public abstract class LibraryItem : IItem, IDescribable
{
    public abstract string Id { get; init; }

    public abstract string DisplayName { get; }

    public abstract string Describe();
}
