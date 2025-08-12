using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DiscogsYoutube.Shared;

public record Format
{
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("qty")]
    public string Qty { get; init; }

    [JsonPropertyName("descriptions")]
    public List<string> Descriptions { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }
}
