namespace Capstone.Core.Model;

public sealed class Album : IDescribable
{
    private readonly string _id = "";
    private readonly string _title = "";
    private readonly string _artist = "";

    public required string Id
    {
        get => _id;
        init => _id = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("An album needs an Id.", nameof(Id))
            : value;
    }

    public required string Title
    {
        get => _title;
        init => _title = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("An album needs a title.", nameof(Title))
            : value.Trim();
    }

    public required string Artist
    {
        get => _artist;
        init => _artist = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("An album needs an artist.", nameof(Artist))
            : value.Trim();
    }

    public required int ReleaseYear { get; init; }

    public string Describe() => $"{Title} by {Artist} ({ReleaseYear})";

    public override string ToString() => Describe();
}
