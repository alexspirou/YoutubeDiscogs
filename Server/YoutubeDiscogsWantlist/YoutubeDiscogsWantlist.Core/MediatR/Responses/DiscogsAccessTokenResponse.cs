namespace YoutubeDiscogsWantlist.MediatR.Responses;


public readonly record struct DiscogsAccessTokenResponse(
    string OAuthToken,
    string OAuthTokenSecret,
    string Username);