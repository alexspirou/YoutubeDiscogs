using System.Text.Json.Serialization;

namespace YoutubeDiscogsWantlist.MediatR.Responses;

public readonly record struct DiscogsRelease
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("country")]
    public string? Country { get; init; }

    [JsonPropertyName("year")]
    public string? Year { get; init; }

    [JsonPropertyName("format")]
    public List<string>? Format { get; init; }

    //[JsonPropertyName("label")]
    //public List<string>? Label { get; init; }

    //[JsonPropertyName("genre")]
    //public List<string>? Genre { get; init; }

    [JsonPropertyName("style")]
    public List<string>? Style { get; init; }

    [JsonPropertyName("cover_image")]
    public string? CoverImage { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("resource_url")]
    public string? ResourceUrl { get; init; }
}