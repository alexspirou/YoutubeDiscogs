using MediatR;
using YoutubeDiscogsWantlist.MediatR.Requests;
using YoutubeDiscogsWantlist.MediatR.Responses;
using YoutubeDiscogsWantlist.Services;

namespace YoutubeDiscogsWantlist.MediatR.Handlers;

public class RequestTokenHandler : IRequestHandler<RequestToken, RequestTokenResponse>
{
    private readonly DiscogsAuthorizationService _discogsAuthorizationService;
    public RequestTokenHandler(DiscogsAuthorizationService discogsAuthorizationService)
    {
        _discogsAuthorizationService = discogsAuthorizationService;
    }
    public async Task<RequestTokenResponse> Handle(RequestToken request, CancellationToken cancellationToken)
    {
        var result = await _discogsAuthorizationService.GetRequestTokenAsync();
        return result;
    }
}
