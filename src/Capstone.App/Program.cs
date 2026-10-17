using System.Text.Json;
using Capstone.Core.Json;
using Capstone.Core.Model;
using Capstone.Core.Storage;

// Loads the music library from data/library.json, skips invalid tracks, and prints what's left.
// dotnet run --project src/Capstone.App

var library = new Library();
var albums = new List<Album>();
var path = Path.Combine(AppContext.BaseDirectory, "data", "library.json");

using (var doc = JsonDocument.Parse(File.ReadAllText(path)))
{
    foreach (var a in doc.RootElement.GetProperty("albums").EnumerateArray())
        albums.Add(new Album
        {
            Id = a.GetProperty("id").GetString()!,
            Title = a.GetProperty("title").GetString()!,
            Artist = a.GetProperty("artist").GetString()!,
            ReleaseYear = a.GetProperty("releaseYear").GetInt32(),
        });

    var mapper = new TrackMapper();
    foreach (var element in doc.RootElement.GetProperty("tracks").EnumerateArray())
    {
        if (mapper.FromJson(element.GetRawText()) is { } track)
            library.Add(track);
        else
            Console.WriteLine($"skipped invalid track: {element.GetRawText()}");
    }
}

Console.WriteLine($"{albums.Count} albums, {library.Count} tracks loaded.");
foreach (var album in albums)
    Console.WriteLine($"  {album.Describe()}");

Console.WriteLine();
foreach (var item in library.All().Take(5))
    Console.WriteLine($"  {(item as IDescribable)?.Describe() ?? item.DisplayName}");
