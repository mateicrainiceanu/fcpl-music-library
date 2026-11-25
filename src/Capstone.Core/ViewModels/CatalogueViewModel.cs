using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Capstone.Core.Model;
using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.ViewModels;

/// <summary>
/// Everything the library window shows, and nothing about how it is shown.
/// The window binds to these properties; this class has never heard of a Button.
/// </summary>
public sealed class CatalogueViewModel : ICatalogueViewModel, IDisposable
{
    private readonly Library _library;
    private readonly ObservableCollection<IItem> _items = [];
    private readonly RelayCommand _addCommand;
    private readonly RelayCommand _removeCommand;
    private readonly RelayCommand _addNewTrackCommand;
    private readonly RelayCommand _playCommand;
    private readonly RelayCommand _rateCommand;
    private string _filter = "";
    private IItem? _selected;

    public CatalogueViewModel(Library library)
    {
        _library = library ?? throw new ArgumentNullException(nameof(library));
        _addCommand = new RelayCommand(Add, p => p is IItem);
        _removeCommand = new RelayCommand(_ => RemoveSelected(), _ => _selected is not null);
        _addNewTrackCommand = new RelayCommand(_ => AddNewTrack());
        _playCommand = new RelayCommand(_ => PlaySelected(), _ => _selected is Track);
        _rateCommand = new RelayCommand(RateSelected, p => _selected is Track && TryReadRating(p, out _));

        _library.ItemAdded += OnItemAdded;   // Lab 06's event, doing the job it was written for
        Refresh();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Always the same collection: a binding holds on to the instance it was given.</summary>
    public IEnumerable<IItem> Items => _items;

    public string Filter
    {
        get => _filter;
        set
        {
            var normalized = value?.Trim() ?? "";
            if (normalized == _filter) return;
            _filter = normalized;
            OnPropertyChanged();
            Refresh();
        }
    }

    public IItem? Selected
    {
        get => _selected;
        set
        {
            if (ReferenceEquals(value, _selected)) return;
            _selected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Details));
            _removeCommand.RaiseCanExecuteChanged();
            _playCommand.RaiseCanExecuteChanged();
            _rateCommand.RaiseCanExecuteChanged();
        }
    }

    public string Details => _selected switch
    {
        null => "Nothing selected.",
        LibraryItem item => item.Describe(),
        var item => item.DisplayName,
    };

    public int Count => _items.Count;

    public ICommand AddCommand => _addCommand;

    public ICommand RemoveCommand => _removeCommand;

    /// <summary>The "new track" form in the window.</summary>
    public TrackForm NewTrack { get; } = new();

    /// <summary>The form's Add button: builds a track from <see cref="NewTrack"/> and adds it.</summary>
    public ICommand AddNewTrackCommand => _addNewTrackCommand;

    /// <summary>The Play button: one more play for the selected track. Greyed out for albums.</summary>
    public ICommand PlayCommand => _playCommand;

    /// <summary>
    /// The star buttons. The parameter is the rating, 1 to 5 (an int, or the text "3" as XAML passes it);
    /// null or "" clears it back to "not rated yet". Greyed out unless a track is selected.
    /// </summary>
    public ICommand RateCommand => _rateCommand;

    private bool Matches(IItem item) =>
        _filter.Length == 0 || item.DisplayName.Contains(_filter, StringComparison.OrdinalIgnoreCase);

    /// <summary>Refills the one collection from the store, in store order, through the filter.</summary>
    private void Refresh()
    {
        var before = _items.Count;
        _items.Clear();
        foreach (var item in _library)
            if (Matches(item)) _items.Add(item);

        if (_selected is not null && !_items.Contains(_selected))
            Selected = null;   // the filter hid it: don't keep describing a row the user can't see
        if (_items.Count != before)
            OnPropertyChanged(nameof(Count));
    }

    private void Add(object? parameter)
    {
        if (parameter is IItem item)
            _library.Add(item);   // the store's rules apply; OnItemAdded updates the list
    }

    private void AddNewTrack()
    {
        if (NewTrack.TryBuild() is not { } track) return;
        try
        {
            AddCommand.Execute(track);
        }
        catch (ArgumentException e)
        {
            NewTrack.Error = e.Message;   // the store said no (a duplicate Id, say): show it, don't crash
            return;
        }
        NewTrack.Clear();
        Selected = track;
    }

    private void PlaySelected()
    {
        if (_selected is not Track track) return;
        track.PlayCount++;
        OnPropertyChanged(nameof(Details));
    }

    private void RateSelected(object? parameter)
    {
        if (_selected is not Track track || !TryReadRating(parameter, out var rating)) return;
        if (track.Rating == rating) return;
        track.Rating = rating;   // Track still guards its own rule: 1 to 5, or null
        OnPropertyChanged(nameof(Details));
    }

    /// <summary>1 to 5, or null for "clear". Anything else isn't a rating.</summary>
    private static bool TryReadRating(object? parameter, out int? rating)
    {
        rating = null;
        switch (parameter)
        {
            case null:
            case string s when string.IsNullOrWhiteSpace(s):
                return true;
            case int n when n is >= 1 and <= 5:
                rating = n;
                return true;
            case string s when int.TryParse(s, out var n) && n is >= 1 and <= 5:
                rating = n;
                return true;
            default:
                return false;
        }
    }

    private void RemoveSelected()
    {
        if (_selected is not { } doomed) return;
        if (_library.Remove(doomed.Id) && _items.Remove(doomed))
            OnPropertyChanged(nameof(Count));
        Selected = null;
    }

    private void OnItemAdded(IItem item)
    {
        if (!Matches(item)) return;
        _items.Add(item);
        OnPropertyChanged(nameof(Count));
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    /// <summary>Stops listening to the library, so a closed window doesn't keep reacting to it.</summary>
    public void Dispose() => _library.ItemAdded -= OnItemAdded;
}
