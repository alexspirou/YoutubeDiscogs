using System.Text.Json.Serialization;

namespace DiscogsYoutube.Shared;

public readonly record struct Pagination
{
    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("pages")]
    public int Pages { get; init; }

    [JsonPropertyName("per_page")]
    public int PerPage { get; init; }

    [JsonPropertyName("items")]
    public int Items { get; init; }
}