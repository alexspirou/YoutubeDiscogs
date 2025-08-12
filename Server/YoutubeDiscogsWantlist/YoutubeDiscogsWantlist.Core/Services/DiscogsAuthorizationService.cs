using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using YoutubeDiscogsWantlist.MediatR.Responses;
using YoutubeDiscogsWantlist.Settings;

namespace YoutubeDiscogsWantlist.Services;

public class DiscogsAuthorizationService
{
    private readonly HttpClient _httpClient;
    private readonly DiscogsSettings _settings;

    public DiscogsAuthorizationService(HttpClient httpClient, IOptions<DiscogsSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }
    public static string CreateOAuth1Header(
    string method,
    string url,
    string consumerKey,
    string consumerSecret,
    string token = "",
    string tokenSecret = "",
    IDictionary<string, string>? extraParams = null)
    {
        var oauth = new SortedDictionary<string, string>
        {
            ["oauth_consumer_key"] = consumerKey,
            ["oauth_nonce"] = Guid.NewGuid().ToString("N"),
            ["oauth_signature_method"] = "HMAC-SHA1",
            ["oauth_timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            ["oauth_version"] = "1.0"
        };

        if (!string.IsNullOrWhiteSpace(token))
            oauth["oauth_token"] = token;

        if (extraParams != null)
        {
            foreach (var kvp in extraParams)
                oauth[kvp.Key] = kvp.Value;
        }

        var paramStr = string.Join("&", oauth.Select(p =>
            $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

        var baseStr = $"{method.ToUpper()}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(paramStr)}";
        var signingKey = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(tokenSecret ?? "")}";

        using var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
        var sig = Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(baseStr)));
        oauth["oauth_signature"] = sig;

        return "OAuth " + string.Join(", ", oauth.Select(p =>
            $"{p.Key}=\"{Uri.EscapeDataString(p.Value)}\""));
    }
    public async Task<RequestTokenResponse> GetRequestTokenAsync()
    {
        var uri = "https://api.discogs.com/oauth/request_token";
        var method = "GET";

        var extraParams = new Dictionary<string, string>
        {
            ["oauth_callback"] = "oob"
        };

        var authHeader = CreateOAuth1Header(
            method,
            uri,
            _settings.ConsumerKey,
            _settings.ConsumerSecret,
            extraParams: extraParams);

        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Add("Authorization", authHeader);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var parsed = HttpUtility.ParseQueryString(content);

        return new RequestTokenResponse
        {
            Token = parsed["oauth_token"],
            TokenSecret = parsed["oauth_token_secret"],
            CallbackConfirmed = parsed["oauth_callback_confirmed"] == "true"
        };
    }

    public async Task<(string AuthorizeUrl, string RequestToken, string RequestTokenSecret)> GetAuthorizeUrlWithTokenAsync()
    {
        var response = await GetRequestTokenAsync();

        if (string.IsNullOrWhiteSpace(response.Token) || string.IsNullOrWhiteSpace(response.TokenSecret))
            throw new InvalidOperationException("Discogs did not return a valid token pair.");

        var authorizeUrl = $"https://discogs.com/oauth/authorize?oauth_token={Uri.EscapeDataString(response.Token)}";

        return (authorizeUrl, response.Token, response.TokenSecret);
    }

    public async Task<DiscogsAccessTokenResponse> ExchangeAccessTokenAsync(
     string requestToken,
     string requestTokenSecret,
     string verifier)
    {
        var uri = "https://api.discogs.com/oauth/access_token";
        var method = "POST";

        var extraParams = new Dictionary<string, string>
        {
            ["oauth_verifier"] = verifier
        };

        var authHeader = CreateOAuth1Header(
            method,
            uri,
            _settings.ConsumerKey,
            _settings.ConsumerSecret,
            requestToken,
            requestTokenSecret,
            extraParams);

        var req = new HttpRequestMessage(HttpMethod.Post, uri);
        req.Headers.Add("Authorization", authHeader);

        var resp = await _httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadAsStringAsync();
        var parsed = HttpUtility.ParseQueryString(body);

        return new DiscogsAccessTokenResponse(
            parsed["oauth_token"] ?? throw new("Missing oauth_token"),
            parsed["oauth_token_secret"] ?? throw new("Missing oauth_token_secret"),
            parsed["username"] ?? "unknown");
    }



    public static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<DiscogsIdentityResponse> GetIdentity(string token, string tokenSecret)
    {
        var uri = "https://api.discogs.com/oauth/identity";
        var method = "GET";

        var authHeader = CreateOAuth1Header(
            method,
            uri,
            _settings.ConsumerKey,
            _settings.ConsumerSecret,
            token,
            tokenSecret);

        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Add("Authorization", authHeader);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<DiscogsIdentityResponse>(json, _options);

        return result;
    }



}
