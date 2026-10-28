using Capstone.Core.Model;

namespace Capstone.Core.Json;

/// <summary>
/// The shape of a track on disk. Everything is nullable on purpose: JSON from outside
/// can be missing anything, and the mapper decides what is acceptable.
/// </summary>
public sealed record TrackDto(
    string? Id,
    string? Title,
    string? Artist,
    Genre? Genre,
    TimeSpan? Duration,
    int? Year,
    int? PlayCount,
    int? Rating,
    string? AlbumId);
