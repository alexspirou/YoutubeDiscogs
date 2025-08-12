using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YoutubeDiscogsWantlist.Core.WantList;

namespace YoutubeDiscogsWantlist.Core.Efcore.Configuration;


public class WantListItemConfiguration : IEntityTypeConfiguration<WantListItem>
{
    public void Configure(EntityTypeBuilder<WantListItem> builder)
    {
        builder.ToTable("WantListItems");

    }
}
