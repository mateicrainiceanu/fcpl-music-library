using SharpQuest.Contracts;

namespace Capstone.Core.Model;

public sealed class Track : IItem, IDescribable
{
    private readonly string _id = "";
    private readonly string _title = "";
    private readonly TimeSpan _duration;
    private int _playCount;
    private int? _rating;

    public required string Id
    {
        get => _id;
        init => _id = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A track needs an Id.", nameof(Id))
            : value;
    }

    public required string Title
    {
        get => _title;
        init => _title = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A track needs a title.", nameof(Title))
            : value.Trim();
    }

    public required Credit Credit { get; init; }

    public required Genre Genre { get; init; }

    public required TimeSpan Duration
    {
        get => _duration;
        init => _duration = value > TimeSpan.Zero
            ? value
            : throw new ArgumentOutOfRangeException(nameof(Duration), value, "A track has a positive length.");
    }

    public int Year { get; init; }

    /// <summary>Id of the album this track belongs to; null for a single.</summary>
    public string? AlbumId { get; init; }

    public int PlayCount
    {
        get => _playCount;
        set => _playCount = value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(PlayCount), value, "Play count cannot be negative.");
    }

    /// <summary>1 to 5 stars. Null means "not rated yet", which is not the same as 0 stars.</summary>
    public int? Rating
    {
        get => _rating;
        set => _rating = value is null or (>= 1 and <= 5)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(Rating), value, "Rating is 1 to 5, or null for unrated.");
    }

    public string Artist => Credit.Artist;

    public string DisplayName => $"{Title} — {Artist}";

    public string Describe()
    {
        var rating = Rating is { } r ? $"{r}★" : "not rated yet";
        return $"{Title} by {Credit} ({Genre}, {Year}) · {Duration:m\\:ss} · {PlayCount} plays · {rating}";
    }

    public override string ToString() => $"[{Id}] {DisplayName}";
}
