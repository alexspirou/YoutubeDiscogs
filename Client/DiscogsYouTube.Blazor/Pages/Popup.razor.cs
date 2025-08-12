namespace DiscogsYouTube.Blazor.Pages;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public partial class Popup : ComponentBase
{
    [Inject] public YoutubeInterop YoutubeInterop { get; set; } = default!;
    [Inject] public HttpClient Http { get; set; } = default!;

    protected List<DiscogsRelease> Releases { get; set; } = new();
    protected string? VideoUrl;
    protected string? VideoTitle;
    protected string? EditableTitle;
    protected string? Error;
    protected bool loading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var info = await YoutubeInterop.GetYoutubeInfo();
            VideoUrl = info.url;
            VideoTitle = info.title;
            EditableTitle = info.title;
            await FetchReleases();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        loading = false;
    }

    protected async Task FetchReleases()
    {
        try
        {
            loading = true;
            var res = await Http.GetFromJsonAsync<DiscogsSearchResponse>(
                $"api/discogs/search?query={Uri.EscapeDataString(EditableTitle)}");
            Releases = res?.Results ?? new();
        }
        catch
        {
            Error = "Failed to fetch releases.";
        }
        loading = false;
    }

    protected void AddToWantlist(string title)
    {
        Console.WriteLine($"'{title}' added to wantlist (mock)");
    }

    public class DiscogsSearchResponse
    {
        public List<DiscogsRelease> Results { get; set; } = new();
    }

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
}
