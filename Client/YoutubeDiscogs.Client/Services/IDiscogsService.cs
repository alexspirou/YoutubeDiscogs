namespace YoutubeDiscogs.Client.Services;

using DiscogsYoutube.Shared.Responses;


public interface IDiscogsService
{
    Task<DiscogsSearchResponse> SearchReleasesAsync(string query, int page = 1, CancellationToken cancellationToken = default);

}
