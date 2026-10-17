using Capstone.Core.Model;

namespace Capstone.Core.Wiring;

internal static class SampleTracks
{
    private static readonly (string Title, string Artist, Genre Genre)[] Pool =
    [
        ("Time", "Pink Floyd", Genre.Rock),
        ("So What", "Miles Davis", Genre.Jazz),
        ("Get Lucky", "Daft Punk", Genre.Electronic),
        ("Lithium", "Nirvana", Genre.Rock),
        ("Money Trees", "Kendrick Lamar", Genre.HipHop),
    ];

    public static Track Create(int seed)
    {
        var (title, artist, genre) = Pool[Math.Abs(seed) % Pool.Length];
        return new Track
        {
            Id = $"trk-{seed:D3}",
            Title = title,
            Credit = new Credit(artist),
            Genre = genre,
            Duration = TimeSpan.FromSeconds(120 + Math.Abs(seed) % 300),
            Year = 1970 + Math.Abs(seed) % 50,
        };
    }
}
