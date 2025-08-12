using System.Text.Json.Serialization;

namespace DiscogsYoutube.Shared;

public record WantlistResponse
{
    [JsonPropertyName("pagination")]
    public Pagination Pagination { get; init; }

    [JsonPropertyName("wants")]
    public List<Want> Wants { get; init; }


}
