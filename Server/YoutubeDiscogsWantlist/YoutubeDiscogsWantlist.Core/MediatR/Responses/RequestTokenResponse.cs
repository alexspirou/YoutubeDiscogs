using System.Text.Json.Serialization;

namespace YoutubeDiscogsWantlist.MediatR.Responses;

public record struct RequestTokenResponse
{
    [JsonPropertyName("oauth_token")]
    public required string Token { get; init; }

    [JsonPropertyName("oauth_token_secret")]
    public required string TokenSecret { get; init; }

    [JsonPropertyName("oauth_callback_confirmed")]
    public bool CallbackConfirmed { get; init; }
}
