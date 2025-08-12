using YoutubeDiscogsWantlist.MediatR.Responses;

namespace YoutubeDiscogsWantlist.Services;

public interface IDiscogsAuthorizationService
{
    Task<RequestTokenResponse> GetRequestTokenAsync();
}
