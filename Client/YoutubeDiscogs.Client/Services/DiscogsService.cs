using DiscogsYoutube.Shared.Responses;
using System.Net.Http.Json;

namespace YoutubeDiscogs.Client.Services;

public sealed class DiscogsService : IDiscogsService
{
    private readonly HttpClient _httpClient;

    public DiscogsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://localhost:7043/");
    }


    public async Task<DiscogsSearchResponse> SearchReleasesAsync(
        string query,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Build the relative URI once
        var requestUri = new Uri(
            $"api/discogs/search?query={Uri.EscapeDataString(query)}&page={page}",
            UriKind.Relative);

        // 1️⃣ Send request
        var httpResponse = await _httpClient.GetAsync(
            requestUri: requestUri,
            cancellationToken: cancellationToken);

        // 2️⃣ Throw on non-success
        httpResponse.EnsureSuccessStatusCode();

        // 3️⃣ Deserialize body with source-generated metadata
        var payload = await httpResponse.Content.ReadFromJsonAsync(
                          jsonTypeInfo: DiscogsJsonContext.Default.DiscogsSearchResponse,
                          cancellationToken: cancellationToken);
        // 4️⃣ Return results (empty list if none)
        return payload;
    }


}