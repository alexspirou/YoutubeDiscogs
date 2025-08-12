using System.Text.Json.Serialization;

namespace DiscogsYoutube.Shared;

public record Artist
{
    [JsonPropertyName("join")]
    public string Join { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("anv")]
    public string Anv { get; init; }

    [JsonPropertyName("tracks")]
    public string Tracks { get; init; }

    [JsonPropertyName("role")]
    public string Role { get; init; }

    [JsonPropertyName("resource_url")]
    public string ResourceUrl { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }
}
