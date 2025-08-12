using System.Text.Json.Serialization;

namespace DiscogsYoutube.Shared;

public record Want
{
    [JsonPropertyName("rating")]
    public int Rating { get; init; }

    [JsonPropertyName("basic_information")]
    public BasicInformation BasicInformation { get; init; }

    [JsonPropertyName("notes")]
    public string? Notes { get; init; }

    [JsonPropertyName("resource_url")]
    public string ResourceUrl { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }
}
