using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Capstone.Core.Model;

namespace Capstone.Core.ViewModels;

/// <summary>
/// The "new track" form beside the list. The text boxes bind to these properties;
/// <see cref="TryBuild"/> turns what was typed into a <see cref="Track"/>, or explains why it can't.
/// </summary>
public sealed class TrackForm : INotifyPropertyChanged
{
    private string _title = "";
    private string _artist = "";
    private Genre _genre = Genre.Rock;
    private string _duration = "";
    private string _error = "";

    public event PropertyChangedEventHandler? PropertyChanged;

    public static IReadOnlyList<Genre> Genres { get; } = Enum.GetValues<Genre>();

    public string Title { get => _title; set => Set(ref _title, value ?? ""); }

    public string Artist { get => _artist; set => Set(ref _artist, value ?? ""); }

    public Genre Genre { get => _genre; set => Set(ref _genre, value); }

    /// <summary>As typed: "3:45".</summary>
    public string Duration { get => _duration; set => Set(ref _duration, value ?? ""); }

    /// <summary>Why the last attempt failed; empty when there is nothing to complain about.</summary>
    public string Error { get => _error; set => Set(ref _error, value ?? ""); }

    /// <summary>A new track with a fresh Id, or null with <see cref="Error"/> saying what's wrong.</summary>
    public Track? TryBuild()
    {
        if (string.IsNullOrWhiteSpace(Title)) return Fail("Give the track a title.");
        if (string.IsNullOrWhiteSpace(Artist)) return Fail("Give the track an artist.");
        if (!TimeSpan.TryParseExact(Duration.Trim(), @"m\:ss", CultureInfo.InvariantCulture, out var length)
            || length <= TimeSpan.Zero)
            return Fail("Length is minutes:seconds, like 3:45.");

        Error = "";
        return new Track
        {
            Id = $"trk-{Guid.NewGuid():N}"[..12],
            Title = Title,
            Artist = Artist,
            Genre = Genre,
            Duration = length,
            Year = DateTime.Now.Year,
        };
    }

    public void Clear()
    {
        Title = "";
        Artist = "";
        Duration = "";
        Error = "";
    }

    private Track? Fail(string message)
    {
        Error = message;
        return null;
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
