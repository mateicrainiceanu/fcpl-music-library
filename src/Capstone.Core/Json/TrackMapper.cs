using System.Text.Json;
using System.Text.Json.Serialization;
using Capstone.Core.Model;
using SharpQuest.Contracts;

namespace Capstone.Core.Json;

public sealed class TrackMapper : IItemMapper
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public string ToJson(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item is not Track t)
            throw new ArgumentException($"This mapper only knows tracks, not {item.GetType().Name}.", nameof(item));

        return JsonSerializer.Serialize(ToDto(t), Options);
    }

    public IItem? FromJson(string json)
    {
        TrackDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<TrackDto>(json, Options);
        }
        catch (JsonException)
        {
            return null; // not JSON, or not an object, or a value of the wrong type
        }

        return dto is null ? null : FromDto(dto);
    }

    public static TrackDto ToDto(Track t) =>
        new(t.Id, t.Title, t.Credit.Artist, t.Credit.FeaturedArtist, t.Genre, t.Duration, t.Year, t.PlayCount, t.Rating, t.AlbumId);

    /// <summary>Null when the DTO is missing something a track cannot exist without.</summary>
    public static Track? FromDto(TrackDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Id)) return null;
        if (string.IsNullOrWhiteSpace(dto.Title)) return null;
        if (string.IsNullOrWhiteSpace(dto.Artist)) return null;
        if (dto.Genre is not { } genre) return null;
        if (dto.Duration is not { } duration || duration <= TimeSpan.Zero) return null;
        if (dto.PlayCount is < 0) return null;
        if (dto.Rating is < 1 or > 5) return null;

        return new Track
        {
            Id = dto.Id,
            Title = dto.Title,
            Credit = new Credit(dto.Artist, string.IsNullOrWhiteSpace(dto.FeaturedArtist) ? null : dto.FeaturedArtist),
            Genre = genre,
            Duration = duration,
            Year = dto.Year ?? 0,
            PlayCount = dto.PlayCount ?? 0,
            Rating = dto.Rating,
            AlbumId = dto.AlbumId,
        };
    }
}
