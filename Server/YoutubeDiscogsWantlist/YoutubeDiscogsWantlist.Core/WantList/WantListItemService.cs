using Microsoft.EntityFrameworkCore;
using YoutubeDiscogsWantlist.Efcore;

namespace YoutubeDiscogsWantlist.Core.WantList;

public class WantListItemService : IWantListItemService
{
    private readonly AppDbContext _dbContext;

    public WantListItemService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddWantListItems(IList<WantListItem> wantListItems, CancellationToken cancellationToken = default)
    {
        foreach (var wantListItem in wantListItems)
        {
            await _dbContext.AddAsync(wantListItem, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IList<WantListItem>> GetWantListByUserName(
        string userName,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.WantListItems
            .Where(x => x.UserName == userName)
            .Select(x => new WantListItem
            {
                Id = x.Id,
                Artist = x.Artist,
                Title = x.Title,
                Year = x.Year,
                Label = x.Label,
                UserName = x.UserName,
            }).ToListAsync();
    }
}
