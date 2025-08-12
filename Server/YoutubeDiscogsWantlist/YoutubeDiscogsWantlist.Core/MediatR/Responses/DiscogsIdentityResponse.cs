namespace YoutubeDiscogsWantlist.MediatR.Responses;

public readonly record struct DiscogsIdentityResponse(
    int Id,
    string Username,
    string ResourceUrl,
    string ConsumerName
);