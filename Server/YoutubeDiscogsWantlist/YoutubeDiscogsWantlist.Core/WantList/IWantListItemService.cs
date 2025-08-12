namespace YoutubeDiscogsWantlist.Core.WantList;

public interface IWantListItemService
{
    Task AddWantListItems(IList<WantListItem> wantListItem, CancellationToken cancellationToken = default);
    Task<IList<WantListItem>> GetWantListByUserName(string userName, CancellationToken cancellationToken = default);

}
