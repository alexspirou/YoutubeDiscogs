namespace DiscogsYoutube.Shared.Messaging.Contracts;

public class UserAddToWantList
{
    public string UserName { get; set; } = default!;
    public int ReleaseId { get; set; }
    public string VideoUrl { get; set; } = default!;
    public DateTime TimestampsUtc { get; set; }
}
