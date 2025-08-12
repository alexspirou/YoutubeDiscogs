using YoutubeDiscogsWantlist.Efcore.Modelsl;

namespace YoutubeDiscogsWantlist.AppUsers;

public interface IAppUserService
{
    Task CreateUser(string discogsUserName, int discogsId, string oAuthToken, string oAuthSecret, CancellationToken cancellationToken = default);
    Task<DiscogsUser> GetByUserName(string discogsUserName, CancellationToken cancellationToken = default);
}
