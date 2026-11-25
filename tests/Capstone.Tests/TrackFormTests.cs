using Capstone.Core.Model;
using Capstone.Core.Storage;
using Capstone.Core.ViewModels;

namespace Capstone.Tests;

public class TrackFormTests
{
    private static TrackForm Filled(string duration = "3:45") => new()
    {
        Title = "Teardrop", Artist = "Massive Attack", Genre = Genre.Electronic, Duration = duration,
    };

    [Fact]
    public void A_filled_form_builds_a_track()
    {
        var track = Filled().TryBuild();

        Assert.NotNull(track);
        Assert.Equal("Teardrop", track.Title);
        Assert.Equal(Genre.Electronic, track.Genre);
        Assert.Equal(TimeSpan.FromSeconds(225), track.Duration);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0:00")]
    [InlineData("3:75")]
    public void A_bad_length_is_explained_not_thrown(string duration)
    {
        var form = Filled(duration);

        Assert.Null(form.TryBuild());
        Assert.False(string.IsNullOrWhiteSpace(form.Error));
    }

    [Fact]
    public void The_add_button_adds_selects_and_clears_the_form()
    {
        var library = new Library();
        using var vm = new CatalogueViewModel(library);
        vm.NewTrack.Title = "Teardrop";
        vm.NewTrack.Artist = "Massive Attack";
        vm.NewTrack.Duration = "5:29";

        vm.AddNewTrackCommand.Execute(null);

        var added = Assert.Single(library.All());
        Assert.Same(added, vm.Selected);
        Assert.Equal("", vm.NewTrack.Title);
    }

    [Fact]
    public void An_incomplete_form_adds_nothing()
    {
        var library = new Library();
        using var vm = new CatalogueViewModel(library);
        vm.NewTrack.Title = "No artist";

        vm.AddNewTrackCommand.Execute(null);

        Assert.Equal(0, library.Count);
        Assert.NotEqual("", vm.NewTrack.Error);
    }
}
