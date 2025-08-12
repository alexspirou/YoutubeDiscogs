using System.Text.Json.Serialization;

namespace DiscogsYoutube.Shared;

public record Label
{
    [JsonPropertyName("resource_url")]
    public string ResourceUrl { get; init; }

    [JsonPropertyName("entity_type")]
    public string EntityType { get; init; }

    [JsonPropertyName("catno")]
    public string CatNo { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }
}
