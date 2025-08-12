using Microsoft.EntityFrameworkCore;
using YoutubeDiscogsWantlist.Core.Efcore.Configuration;
using YoutubeDiscogsWantlist.Core.WantList;
using YoutubeDiscogsWantlist.Efcore.Configuration;
using YoutubeDiscogsWantlist.Efcore.Modelsl;

namespace YoutubeDiscogsWantlist.Efcore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DiscogsUser> DiscogsUsers => Set<DiscogsUser>();
    public DbSet<WantListItem> WantListItems => Set<WantListItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DiscogsUserConfiguration());
        modelBuilder.ApplyConfiguration(new WantListItemConfiguration());
    }
}
