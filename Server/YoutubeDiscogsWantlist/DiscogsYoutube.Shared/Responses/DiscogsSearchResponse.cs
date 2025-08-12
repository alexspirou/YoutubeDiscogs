using System.Text.Json.Serialization;

namespace DiscogsYoutube.Shared.Responses;
public readonly record struct DiscogsSearchResponse
{
    [JsonPropertyName("pagination")]
    public Pagination? Pagination { get; init; }

    [JsonPropertyName("results")]
    public List<DiscogsRelease>? Results { get; init; }
}