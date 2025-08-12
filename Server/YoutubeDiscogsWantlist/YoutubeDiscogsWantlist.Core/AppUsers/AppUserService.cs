using Microsoft.EntityFrameworkCore;
using YoutubeDiscogsWantlist.Efcore;
using YoutubeDiscogsWantlist.Efcore.Modelsl;
using YoutubeDiscogsWantlist.Exceptions;

namespace YoutubeDiscogsWantlist.AppUsers;

public class AppUserService : IAppUserService
{

    private readonly AppDbContext _dbContext;

    public AppUserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }



    public async Task CreateUser(string discogsUserName, int discogsId, string oAuthToken, string oAuthSecret, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.DiscogsUsers
            .AnyAsync(u => u.DiscogsUsername == discogsUserName, cancellationToken);

        if (exists)
            throw new UserAlreadyExistsException(discogsUserName);

        var user = new DiscogsUser
        {
            DiscogsUsername = discogsUserName,
            DiscogsId = discogsId,
            OAuthToken = oAuthToken,
            OAuthTokenSecret = oAuthSecret,
            CreatedAt = DateTime.UtcNow,

        };

        _dbContext.Add(user);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<DiscogsUser> GetByUserName(string discogsUserName, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.DiscogsUsers
            .FirstOrDefaultAsync(x => x.DiscogsUsername == discogsUserName, cancellationToken);

        if (user is null)
            throw new DiscogsUserNotFoundException(discogsUserName);

        return user;
    }
}
