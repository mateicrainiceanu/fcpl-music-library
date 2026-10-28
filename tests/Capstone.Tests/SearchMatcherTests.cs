using Capstone.Core.Model;
using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Tests;

public class SearchMatcherTests
{
    [Fact]
    public void WhiteSpaceMatchesNonNullSpace()
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("Pikachu", " "));
        Assert.False(matcher.Matches(null, " "));
    }

    [Fact]
    public void IgnoreCase()
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("Pikachu", "PIKA"));
        Assert.True(matcher.Matches("Pikachu", "pika"));
    }

    [Theory]
    [InlineData("cafe", "Café")]
    [InlineData("Café", "cafe")]
    [InlineData("șosea", "Sosea")]
    [InlineData("Sosea", "șosea")]
    public void IgnoreDiacritics(string text, string query)
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches(text, query));
    }

    [Fact]
    public void MultipleWordsDistinctOrder()
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("fire lizard", "lizard fire"));
        Assert.False(matcher.Matches("fire lizard", "lizard water"));
    }

    [Fact]
    public void SubstringShouldMatch()
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("Pikachu", "chu"));
    }

    [Fact]
    public void NullTextDoesNotMatch()
    {
        var matcher = SearchMatcher.Create();
        Assert.False(matcher.Matches(null, "some random txt"));
    }

    [Fact]
    public void ArgumentNullExceptionIsThrown()
    {
        var matcher = SearchMatcher.Create();
        Assert.Throws<ArgumentNullException>(() => matcher.Matches("text", null!));
    }

    [Fact]
    public void TrackRejectsEmptyId()
    {
        Assert.Throws<ArgumentException>(() => MakeTrack(""));
    }

    [Fact]
    public void LibraryRejectsDuplicateId()
    {
        var library = new Library();
        library.Add(MakeTrack("1"));
        Assert.Throws<ArgumentException>(() => library.Add(MakeTrack("1")));
    }

    [Fact]
    public void TrackDisplayNameIsTitleAndArtist()
    {
        Assert.Equal("Time — Pink Floyd", MakeTrack("1").DisplayName);
    }

    [Fact]
    public void RatingNullMeansUnratedAndZeroIsRejected()
    {
        var t = MakeTrack("1");
        Assert.Null(t.Rating);
        Assert.Throws<ArgumentOutOfRangeException>(() => t.Rating = 0);
        t.Rating = 5;
        Assert.Equal(5, t.Rating);
    }

    [Fact]
    public void PlayCountCannotBeNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MakeTrack("1").PlayCount = -1);
    }

    private static Track MakeTrack(string id) => new()
    {
        Id = id,
        Title = "Time",
        Artist = "Pink Floyd",
        Genre = Genre.Rock,
        Duration = TimeSpan.FromSeconds(413),
        Year = 1973,
    };
}
