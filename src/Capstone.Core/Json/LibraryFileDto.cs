namespace Capstone.Core.Json;

/// <summary>
/// The whole file. <see cref="Format"/> is how a loader tells "my file" from "some other JSON".
/// </summary>
public sealed record LibraryFileDto(
    string? Format,
    int? Version,
    List<StoredItemDto?>? Items);

/// <summary>
/// One item, tagged with its kind. Only the slot matching <see cref="Kind"/> is filled in,
/// so each kind is written with all of its own fields instead of just what IItem declares.
/// </summary>
public sealed record StoredItemDto(
    string? Kind,
    TrackDto? Track = null,
    AlbumDto? Album = null);
