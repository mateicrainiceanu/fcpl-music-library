using System.Text.Json;
using Capstone.Core.Json;
using Capstone.Core.Model;

namespace Capstone.Core.Storage;

/// <summary>The bundled sample (data/library.json): what a brand-new install starts with.</summary>
public static class SampleLibrary
{
    /// <summary>
    /// Adds the sample's albums, then its tracks, to <paramref name="library"/>.
    /// Tracks the mapper rejects are skipped and reported through <paramref name="skipped"/>.
    /// </summary>
    public static void AddTo(Library library, string path, Action<string>? skipped = null)
    {
        ArgumentNullException.ThrowIfNull(library);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        foreach (var a in doc.RootElement.GetProperty("albums").EnumerateArray())
        {
            library.Add(new Album
            {
                Id = a.GetProperty("id").GetString()!,
                Title = a.GetProperty("title").GetString()!,
                Artist = a.GetProperty("artist").GetString()!,
                ReleaseYear = a.GetProperty("releaseYear").GetInt32(),
            });
        }

        var mapper = new TrackMapper();
        foreach (var element in doc.RootElement.GetProperty("tracks").EnumerateArray())
        {
            if (mapper.FromJson(element.GetRawText()) is { } track)
                library.Add(track);
            else
                skipped?.Invoke(element.GetRawText());
        }
    }
}
