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

public class PlayAndRateTests
{
    private static (Library Library, CatalogueViewModel Vm, Track Track) WithOneTrack()
    {
        var library = new Library();
        var track = new Track
        {
            Id = "t1", Title = "Time", Artist = "Pink Floyd", Genre = Genre.Rock, Duration = TimeSpan.FromMinutes(7),
        };
        library.Add(track);
        library.Add(new Album { Id = "a1", Title = "DSOTM", Artist = "Pink Floyd", ReleaseYear = 1973 });
        return (library, new CatalogueViewModel(library), track);
    }

    [Fact]
    public void Play_counts_a_play_and_updates_the_details()
    {
        var (_, vm, track) = WithOneTrack();
        vm.Selected = track;
        var announced = new List<string?>();
        vm.PropertyChanged += (_, e) => announced.Add(e.PropertyName);

        vm.PlayCommand.Execute(null);

        Assert.Equal(1, track.PlayCount);
        Assert.Contains(nameof(vm.Details), announced);
        Assert.Contains("1 plays", vm.Details);
    }

    [Theory]
    [InlineData("4", 4)]
    [InlineData(2, 2)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Rate_sets_or_clears_the_rating(object? parameter, int? expected)
    {
        var (_, vm, track) = WithOneTrack();
        track.Rating = 3;
        vm.Selected = track;

        vm.RateCommand.Execute(parameter);

        Assert.Equal(expected, track.Rating);
    }

    [Fact]
    public void Play_and_rate_are_greyed_out_for_albums_and_for_nothing()
    {
        var (library, vm, _) = WithOneTrack();
        Assert.False(vm.PlayCommand.CanExecute(null));
        Assert.False(vm.RateCommand.CanExecute("3"));

        vm.Selected = library.FindById("a1");
        Assert.False(vm.PlayCommand.CanExecute(null));
        Assert.False(vm.RateCommand.CanExecute("3"));
    }

    [Fact]
    public void A_rating_out_of_range_is_refused_quietly()
    {
        var (_, vm, track) = WithOneTrack();
        vm.Selected = track;

        Assert.False(vm.RateCommand.CanExecute("9"));
        vm.RateCommand.Execute("9");

        Assert.Null(track.Rating);
    }
}
