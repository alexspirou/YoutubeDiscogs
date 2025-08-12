namespace YoutubeDiscogsWantlist.Efcore.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YoutubeDiscogsWantlist.Efcore.Modelsl;

public class DiscogsUserConfiguration : IEntityTypeConfiguration<DiscogsUser>
{
    public void Configure(EntityTypeBuilder<DiscogsUser> builder)
    {
        builder.ToTable("DiscogsUsers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.DiscogsUsername)
            .IsRequired()
            .HasMaxLength(100);


        builder.Property(x => x.DiscogsId)
            .IsRequired();

        builder.Property(x => x.OAuthToken)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.OAuthTokenSecret)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
