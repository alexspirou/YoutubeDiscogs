using MediatR;
using YoutubeDiscogsWantlist.MediatR.Responses;

namespace YoutubeDiscogsWantlist.MediatR.Requests;

public record struct RequestToken() : IRequest<RequestTokenResponse>;

