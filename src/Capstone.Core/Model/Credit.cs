namespace Capstone.Core.Model;

/// <summary>Who made a track: the main artist and, optionally, a featured one.</summary>
public sealed record Credit(string Artist, string? FeaturedArtist = null)
{
    public override string ToString() =>
        FeaturedArtist is null ? Artist : $"{Artist} feat. {FeaturedArtist}";
}
