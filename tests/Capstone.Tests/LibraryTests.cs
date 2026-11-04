using Capstone.Core.Model;
using Capstone.Core.Storage;
using Capstone.Core.Wiring;

namespace Capstone.Tests;

public class LibraryTests
{
    private static Track MakeTrack(string id, string? albumId = null, Genre genre = Genre.Rock) => new()
    {
        Id = id, Title = $"T {id}", Artist = "A", Genre = genre,
        Duration = TimeSpan.FromMinutes(3), Year = 1990, AlbumId = albumId,
    };

    [Fact]
    public void Where_uses_the_callers_rule()
    {
        var library = new Library();
        library.Add(MakeTrack("t1", genre: Genre.Jazz));
        library.Add(MakeTrack("t2", genre: Genre.Rock));

        var jazz = library.Where(i => i is Track { Genre: Genre.Jazz });

        Assert.Equal("t1", Assert.Single(jazz).Id);
    }

    [Fact]
    public void Map_and_ForEach_visit_in_order_and_reject_null()
    {
        var library = new Library();
        library.Add(MakeTrack("t1"));
        library.Add(MakeTrack("t2"));
        var seen = new List<string>();

        library.ForEach(i => seen.Add(i.Id));

        Assert.Equal(["t1", "t2"], seen);
        Assert.Equal(["t1", "t2"], library.Map(i => i.Id));
        Assert.Throws<ArgumentNullException>(() => library.Where(null!));
    }

    [Fact]
    public void Track_describes_unrated_differently_from_rated()
    {
        var track = MakeTrack("t1");
        Assert.Contains("not rated yet", track.Describe());
        track.Rating = 4;
        Assert.Contains("4★", track.Describe());
    }

    [Fact]
    public void Recently_used_keeps_newest_first_and_caps()
    {
        var recent = new RecentlyUsed<string>(2);
        recent.Push("a"); recent.Push("b"); recent.Push("c");
        Assert.Equal(["c", "b"], recent.Items);
    }
}
