namespace YoutubeDiscogs.Client.Services;

using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;

public class YoutubeInterop
{
    private readonly IJSRuntime _js;

    public YoutubeInterop(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<(string url, string title)> GetYoutubeInfo()
    {
        var result = await _js.InvokeAsync<Dictionary<string, object>>("chromeInterop.getActiveTabUrlAndTitle");
        return (result["url"].ToString(), result["title"].ToString());
    }
}
