namespace Capstone.Core.Json;

/// <summary>The shape of an album on disk. Nullable for the same reason as <see cref="TrackDto"/>.</summary>
public sealed record AlbumDto(
    string? Id,
    string? Title,
    string? Artist,
    int? ReleaseYear);
