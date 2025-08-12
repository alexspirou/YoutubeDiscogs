namespace YoutubeDiscogsWantlist.Core.Settings;

public class GoogleSettings
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string RedirectUri { get; set; } = "";
    public string Scopes { get; set; } = "https://www.googleapis.com/auth/youtube.readonly";
}