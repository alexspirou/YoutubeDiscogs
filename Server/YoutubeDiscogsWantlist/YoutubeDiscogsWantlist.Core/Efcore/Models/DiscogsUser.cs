namespace YoutubeDiscogsWantlist.Efcore.Modelsl;

public class DiscogsUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string DiscogsUsername { get; set; }
    public required int DiscogsId { get; set; }
    public required string OAuthToken { get; set; } = null!;
    public required string OAuthTokenSecret { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}