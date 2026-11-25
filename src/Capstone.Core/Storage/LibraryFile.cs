using System.Text.Json;
using Capstone.Core.Json;
using Capstone.Core.Model;
using SharpQuest.Contracts;

namespace Capstone.Core.Storage;

/// <summary>Saves a library to a JSON file and reads it back.</summary>
public static class LibraryFile
{
    private const string FormatName = "capstone-library";
    private const int CurrentVersion = 1;
    private const string TrackKind = "track";
    private const string AlbumKind = "album";

    /// <summary>Where both the console app and the window keep the library between runs.</summary>
    public static string DefaultPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Capstone", "library.json");

    /// <summary>
    /// Writes a snapshot of the store to <paramref name="path"/>, replacing what was there.
    /// It writes to a temporary file first and then moves it into place, so a crash halfway
    /// through leaves the old file intact rather than half a new one.
    /// </summary>
    public static async Task SaveAsync(IItemStore store, string path)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // Build the DTOs now: this is the snapshot, taken before any await.
        var file = new LibraryFileDto(FormatName, CurrentVersion, store.All().Select(ToStored).ToList<StoredItemDto?>());

        var folder = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(folder))
            Directory.CreateDirectory(folder);

        var temp = path + ".tmp";
        await using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None,
                         bufferSize: 4096, useAsync: true))
        {
            await JsonSerializer.SerializeAsync(stream, file, TrackMapper.Options);
        }

        File.Move(temp, path, overwrite: true);
    }

    /// <summary>
    /// The library as it was saved. No file means nothing saved yet, so an empty library.
    /// A file that is there but can't be read as ours is <see cref="InvalidDataException"/>.
    /// </summary>
    public static async Task<Library> LoadAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
            return new Library();

        LibraryFileDto? file;
        try
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: true);
            file = await JsonSerializer.DeserializeAsync<LibraryFileDto>(stream, TrackMapper.Options);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"'{Path.GetFileName(path)}' is not valid JSON: {ex.Message}", ex);
        }

        if (file is null || file.Format != FormatName || file.Items is null)
            throw new InvalidDataException($"'{Path.GetFileName(path)}' is JSON, but not a saved library.");
        if (file.Version != CurrentVersion)
            throw new InvalidDataException($"'{Path.GetFileName(path)}' is version {file.Version}; this program reads version {CurrentVersion}.");

        var library = new Library();
        for (var i = 0; i < file.Items.Count; i++)
        {
            var item = FromStored(file.Items[i])
                ?? throw new InvalidDataException($"Item {i} in '{Path.GetFileName(path)}' is incomplete or of an unknown kind.");
            try
            {
                library.Add(item);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidDataException($"Item {i} in '{Path.GetFileName(path)}': {ex.Message}", ex);
            }
        }
        return library;
    }

    private static StoredItemDto ToStored(IItem item) => item switch
    {
        Track t => new StoredItemDto(TrackKind, Track: TrackMapper.ToDto(t)),
        Album a => new StoredItemDto(AlbumKind, Album: new AlbumDto(a.Id, a.Title, a.Artist, a.ReleaseYear)),
        _ => throw new NotSupportedException($"Don't know how to save a {item.GetType().Name}."),
    };

    /// <summary>Null when the entry is missing something its kind can't exist without.</summary>
    private static IItem? FromStored(StoredItemDto? stored) => stored switch
    {
        { Kind: TrackKind, Track: { } t } => TrackMapper.FromDto(t),
        { Kind: AlbumKind, Album: { } a } => FromDto(a),
        _ => null,
    };

    private static Album? FromDto(AlbumDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Id)) return null;
        if (string.IsNullOrWhiteSpace(dto.Title)) return null;
        if (string.IsNullOrWhiteSpace(dto.Artist)) return null;
        if (dto.ReleaseYear is not { } year) return null;

        return new Album { Id = dto.Id, Title = dto.Title, Artist = dto.Artist, ReleaseYear = year };
    }
}
