using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DiscogsYoutube.Shared;

public record BasicInformation
{
    [JsonPropertyName("formats")]
    public List<Format> Formats { get; init; }

    [JsonPropertyName("thumb")]
    public string Thumb { get; init; }

    [JsonPropertyName("cover_image")]
    public string CoverImage { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; }

    [JsonPropertyName("labels")]
    public List<Label> Labels { get; init; }

    [JsonPropertyName("year")]
    public int Year { get; init; }

    [JsonPropertyName("artists")]
    public List<Artist> Artists { get; init; }

    [JsonPropertyName("resource_url")]
    public string ResourceUrl { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }
}
