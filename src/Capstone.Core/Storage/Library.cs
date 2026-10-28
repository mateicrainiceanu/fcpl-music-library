using System.Collections;
using SharpQuest.Contracts;

namespace Capstone.Core.Storage;

public sealed class Library : IItemStore, IEnumerable<IItem>
{
    private readonly List<IItem> _ordered = [];
    private readonly Dictionary<string, IItem> _byId = new(StringComparer.Ordinal);

    public int Count => _ordered.Count;

    public IItem this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _ordered.Count);
            return _ordered[index];
        }
    }

    public IItem? this[string id] => FindById(id);

    public IEnumerator<IItem> GetEnumerator()
    {
        foreach (var item in _ordered)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerable<IItem> Matching(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        foreach (var item in _ordered)
        {
            if (item.DisplayName.Contains(text, StringComparison.OrdinalIgnoreCase))
                yield return item;
        }
    }

    public void Add(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!_byId.TryAdd(item.Id, item))
            throw new ArgumentException($"An item with Id '{item.Id}' is already in the library.", nameof(item));

        _ordered.Add(item);
    }

    public IItem? FindById(string id) => _byId.GetValueOrDefault(id);

    public IReadOnlyList<IItem> All() => _ordered.ToArray();

    public bool Remove(string id)
    {
        if (!_byId.Remove(id, out var item))
            return false;

        _ordered.Remove(item);
        return true;
    }
}
