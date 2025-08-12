namespace DiscogsYoutube.Shared.Responses;

public class DiscogsRelease
{
    public string Title { get; set; } = "";
    public string Uri { get; set; } = "";
    public string ResourceUrl { get; set; } = "";
    public string? Country { get; set; }
    public string? Year { get; set; }
    public string? CoverImage { get; set; }
    public string? Thumb { get; set; }
}