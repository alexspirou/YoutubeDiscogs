using DiscogsYoutube.Shared;
using DiscogsYoutube.Shared.Responses;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Web;
using YoutubeDiscogsWantlist.Settings;

namespace YoutubeDiscogsWantlist.Services;

public class DiscogsService
{
    private readonly HttpClient _httpClient;
    private readonly DiscogsSettings _settings;
    public DiscogsService(HttpClient httpClient, IOptions<DiscogsSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };


    public async Task<DiscogsSearchResponse> SearchReleasesAsync(string query, int page = 1, int perPage = 10)
    {
        var builder = new UriBuilder("https://api.discogs.com/database/search");
        var queryParams = HttpUtility.ParseQueryString(string.Empty);

        queryParams["q"] = query;
        queryParams["type"] = "release";
        queryParams["per_page"] = perPage.ToString();
        queryParams["page"] = page.ToString();
        queryParams["key"] = _settings.ConsumerKey;
        queryParams["secret"] = _settings.ConsumerSecret;

        builder.Query = queryParams.ToString();

        var response = await _httpClient.GetAsync(builder.Uri);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<DiscogsSearchResponse>(json, _options);

        return result;
    }

    public async Task AddReleaseToWantlistAsync(
    string userName,
    int releaseId,
    string oauthToken,
    string oauthTokenSecret,
    int? notesId = null,
    int? rating = null
)
    {

        // Construct the endpoint URL
        var uri = $"https://api.discogs.com/users/{Uri.EscapeDataString(userName)}/wants/{releaseId}";
        var method = "PUT";

        // Optionally add notes/rating in the JSON body
        string? jsonBody = null;
        if (notesId.HasValue || rating.HasValue)
        {
            var obj = new Dictionary<string, object>();
            if (notesId.HasValue) obj["notes"] = notesId.Value;
            if (rating.HasValue) obj["rating"] = rating.Value;
            jsonBody = JsonSerializer.Serialize(obj);
        }
        else
        {

        }

        // Build the OAuth1 header (reuse your CreateOAuth1Header from DiscogsAuthorizationService)
        var authHeader = DiscogsAuthorizationService.CreateOAuth1Header(
            method,
            uri,
            _settings.ConsumerKey,
            _settings.ConsumerSecret,
            oauthToken,
            oauthTokenSecret
        );

        var request = new HttpRequestMessage(HttpMethod.Put, uri);
        request.Headers.Add("Authorization", authHeader);

        if (jsonBody != null)
        {
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }


    public async Task<WantlistResponse> GetWantlist(string userName, string oauthToken, string oauthTokenSecret, int page, int perPage, CancellationToken ct = default)
    {
        // 1️⃣  Configure the RestClient once (you can keep a single instance)
        var client = new RestClient(new RestClientOptions("https://api.discogs.com/")
        {
            // optional: automatic JSON serialiser settings
            //ConfigureMessageHandler = _ => _httpClient // reuse the same HttpMessageHandler
        });

        // 2️⃣  Build the request
        var request = new RestRequest($"users/{Uri.EscapeDataString(userName)}/wants", Method.Get)
            .AddQueryParameter("page", page.ToString(CultureInfo.InvariantCulture))
            .AddQueryParameter("per_page", perPage.ToString(CultureInfo.InvariantCulture))
            .AddHeader("User-Agent", "YoutubeDiscogs/1.0 (+https://example.com)");

        // 3️⃣  Attach OAuth-1 authenticator (RestSharp handles signature math)
        request.Authenticator = OAuth1Authenticator.ForProtectedResource(
            consumerKey: _settings.ConsumerKey,
            consumerSecret: _settings.ConsumerSecret,
            accessToken: oauthToken,
            accessTokenSecret: oauthTokenSecret,
            OAuthSignatureMethod.HmacSha1);

        // 4️⃣  Execute and automatically deserialize to WantlistResponse
        var response = await client.ExecuteAsync<WantlistResponse>(request, ct);

        // 5️⃣  Error handling
        if (!response.IsSuccessful)
        {
            var msg = response.Content?.Trim() ?? response.StatusDescription;
            throw new HttpRequestException($"Discogs returned {(int)response.StatusCode} – {msg}");
        }

        return response.Data
               ?? throw new InvalidOperationException("Empty want-list payload.");
    }
    //public async Task<WantlistResponse> GetWantlist(string userName, string oauthToken, string oauthTokenSecret, int page, int perPage)
    //{
    //    // Construct the endpoint URL
    //    var uri = $"https://api.discogs.com/users/{Uri.EscapeDataString(userName)}/wants?page=1&per_page=2";

    //    var method = "GET";

    //    var query = new Dictionary<string, string>
    //    {
    //        ["page"] = page.ToString(CultureInfo.InvariantCulture),
    //        ["per_page"] = perPage.ToString(CultureInfo.InvariantCulture)
    //    };

    //    // Build the OAuth1 header (reuse your CreateOAuth1Header from DiscogsAuthorizationService)
    //    var authHeader = DiscogsAuthorizationService.CreateOAuth1Header(
    //        method,
    //        uri,
    //        _settings.ConsumerKey,
    //        _settings.ConsumerSecret,
    //        oauthToken,
    //    oauthTokenSecret,
    //    extraParams: query
    //    );


    //    var request = new HttpRequestMessage(HttpMethod.Get, uri);
    //    request.Headers.Add("Authorization", authHeader);


    //    var response = await _httpClient.SendAsync(request);
    //    response.EnsureSuccessStatusCode();

    //    var content = await response.Content.ReadAsStringAsync();

    //    var wantlist = JsonSerializer.Deserialize<WantlistResponse>(
    //        content,
    //        DiscogsAuthorizationService._options
    //    );


    //    return wantlist;
    //}

    public async IAsyncEnumerable<Want> GetWantlistWithYield(string userName, string oauthToken, string oauthTokenSecret)
    {
        int page = 1;
        int perPage = 10;
        while (true)
        {

            var response = await GetWantlist(userName, oauthToken, oauthTokenSecret, page, perPage);

            if (response.Pagination.Items == 0)
                yield break;


            foreach (var want in response.Wants)
                yield return want;


            if (response.Pagination.Page == response.Pagination.Pages)
                yield break;
            page++;
            Console.WriteLine($"Page increades {page}");

        }

    }
}
