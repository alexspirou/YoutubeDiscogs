namespace YoutubeDiscogs.Client.Pages;

using DiscogsYoutube.Shared.Responses;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeDiscogs.Client.Services;

public partial class Popup : ComponentBase
{
    [Inject] public YoutubeInterop YoutubeInterop { get; set; } = default!;
    [Inject] public IDiscogsService DiscogsService { get; set; } = default!;

    protected IReadOnlyList<DiscogsRelease> Releases { get; set; } = [];
    protected string? EditableTitle { get; set; } = ""; // Start with empty, or set your default
    protected string? Error { get; set; }
    protected bool loading = true;

    protected int CurrentPage { get; set; } = 1;
    protected int TotalPages { get; set; } = 1;
    protected int TotalResults { get; set; } = 0;
    protected int PageSize { get; set; } = 8; // If needed, send to backend (Discogs default is 50)

    protected override async Task OnInitializedAsync()
    {
        loading = false;
    }

    protected async Task FetchReleases()
    {
        try
        {
            loading = true;
            Error = null;
            // If your backend supports pageSize, pass it (e.g. page: CurrentPage, pageSize: PageSize)
            var response = await DiscogsService.SearchReleasesAsync(EditableTitle, CurrentPage);

            Releases = response.Results ?? [];
            TotalResults = response.Pagination.Value.Items;
            TotalPages = response.Pagination.Value.Pages;

            // Defensive fallback if API misbehaves
            if (TotalPages <= 0)
                TotalPages = 1;
            if (CurrentPage > TotalPages)
                CurrentPage = TotalPages;
        }
        catch (Exception ex)
        {
            Releases = [];
            Error = "Failed to fetch releases.";
        }
        finally
        {
            loading = false;
        }
    }

    protected async Task PrevPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await FetchReleases();
        }
    }

    protected async Task NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await FetchReleases();
        }
    }

    protected void AddToWantlist(string title)
    {
        // Real implementation would call an API or service to add the item
        Console.WriteLine($"'{title}' added to wantlist (mock)");
    }
}
