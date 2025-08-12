using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using YoutubeDiscogsWantlist.Core.Settings;

namespace YoutubeDiscogsWantlist.Core.Controllers.Youtube;


[ApiController]
[Route("")]
public class YouTubeOAuthController : ControllerBase
{
    private readonly IHttpClientFactory _http;
    private readonly GoogleSettings _cfg;
    private readonly ILogger<YouTubeOAuthController> _log;

    public YouTubeOAuthController(IHttpClientFactory http, IConfiguration config, ILogger<YouTubeOAuthController> log)
    {
        _http = http;
        _cfg = config.GetSection("Google").Get<GoogleSettings>() ?? new GoogleSettings();
        _log = log;
    }

    // STEP 1: Send user to Google (with PKCE + state)
    // GET /oauth/login
    [HttpGet("oauth/login")]
    public IActionResult Login()
    {
        // Generate anti-CSRF state and PKCE code verifier/challenge
        var state = Base64Url(RandomBytes(32));
        var codeVerifier = Base64Url(RandomBytes(32));
        var codeChallenge = Base64Url(Sha256(codeVerifier));

        HttpContext.Session.SetString("oauth_state", state);
        HttpContext.Session.SetString("code_verifier", codeVerifier);

        var url =
            "https://accounts.google.com/o/oauth2/v2/auth" +
            "?response_type=code" +
            $"&client_id={Uri.EscapeDataString(_cfg.ClientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(_cfg.RedirectUri)}" +
            $"&scope={Uri.EscapeDataString(_cfg.Scopes)}" +
            $"&state={Uri.EscapeDataString(state)}" +
            "&access_type=offline" +            // request refresh token
            "&prompt=consent" +                 // ensure refresh token first time
            "&code_challenge_method=S256" +
            $"&code_challenge={codeChallenge}";

        return Redirect(url);
    }

    // STEP 2: Handle Google's redirect and exchange code for tokens
    // GET /oauth2/callback
    [HttpGet("oauth2/callback")]
    public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error)
    {
        if (!string.IsNullOrEmpty(error))
            return BadRequest(new { error });

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            return BadRequest(new { error = "Missing 'code' or 'state'." });

        var expectedState = HttpContext.Session.GetString("oauth_state");
        if (string.IsNullOrEmpty(expectedState) || !string.Equals(state, expectedState, StringComparison.Ordinal))
            return BadRequest(new { error = "Invalid state." });

        var codeVerifier = HttpContext.Session.GetString("code_verifier") ?? "";
        var http = _http.CreateClient();

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _cfg.ClientId,
            ["client_secret"] = _cfg.ClientSecret,
            ["redirect_uri"] = _cfg.RedirectUri,
            ["grant_type"] = "authorization_code",
            ["code_verifier"] = codeVerifier
        });

        var resp = await http.PostAsync("https://oauth2.googleapis.com/token", form);
        var json = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
        {
            _log.LogError("Token exchange failed: {Status} {Body}", resp.StatusCode, json);
            return Problem($"Token exchange failed: {resp.StatusCode}");
        }

        var tokens = JsonSerializer.Deserialize<TokenResponse>(json);
        if (tokens is null || string.IsNullOrEmpty(tokens.access_token))
            return Problem("Invalid token response.");

        SaveTokens(tokens);
        return Redirect("/youtube/me");
    }

    // STEP 3: Example YouTube call (auto refresh on expiry/401)
    // GET /youtube/me
    [HttpGet("youtube/me")]
    public async Task<IActionResult> GetMyChannel()
    {
        var accessToken = await EnsureAccessTokenAsync();
        if (accessToken is null)
            return Unauthorized(new { error = "Not authenticated. Visit /oauth/login" });

        var http = _http.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var url = "https://www.googleapis.com/youtube/v3/channels?part=snippet,statistics&mine=true";
        var resp = await http.GetAsync(url);

        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized && await TryRefreshAsync())
        {
            accessToken = HttpContext.Session.GetString("access_token");
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            resp = await http.GetAsync(url);
        }

        var json = await resp.Content.ReadAsStringAsync();
        return Content(json, "application/json", Encoding.UTF8);
    }

    // Optional: Clear tokens
    // GET /oauth/logout
    [HttpGet("oauth/logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("access_token");
        HttpContext.Session.Remove("refresh_token");
        HttpContext.Session.Remove("access_token_expires_at");
        HttpContext.Session.Remove("oauth_state");
        HttpContext.Session.Remove("code_verifier");
        return Ok(new { message = "Signed out (local tokens cleared)." });
    }

    // ----------------- Helpers -----------------

    private async Task<string?> EnsureAccessTokenAsync()
    {
        var token = HttpContext.Session.GetString("access_token");
        var expRaw = HttpContext.Session.GetString("access_token_expires_at");

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(expRaw))
            return null;

        var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expRaw));
        if (exp <= DateTimeOffset.UtcNow.AddSeconds(60))
        {
            if (!await TryRefreshAsync()) return null;
            token = HttpContext.Session.GetString("access_token");
        }
        return token;
    }

    private async Task<bool> TryRefreshAsync()
    {
        var refreshToken = HttpContext.Session.GetString("refresh_token");
        if (string.IsNullOrEmpty(refreshToken)) return false;

        var http = _http.CreateClient();
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _cfg.ClientId,
            ["client_secret"] = _cfg.ClientSecret,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        });

        var resp = await http.PostAsync("https://oauth2.googleapis.com/token", form);
        var json = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode) return false;

        var tokens = JsonSerializer.Deserialize<TokenResponse>(json);
        if (tokens is null || string.IsNullOrEmpty(tokens.access_token)) return false;

        tokens.refresh_token ??= refreshToken; // Google may omit it on refresh
        SaveTokens(tokens);
        return true;
    }

    private void SaveTokens(TokenResponse t)
    {
        HttpContext.Session.SetString("access_token", t.access_token);
        if (!string.IsNullOrEmpty(t.refresh_token))
            HttpContext.Session.SetString("refresh_token", t.refresh_token);

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(t.expires_in);
        HttpContext.Session.SetString("access_token_expires_at", expiresAt.ToUnixTimeSeconds().ToString());
    }

    // Crypto/encoding helpers
    private static byte[] RandomBytes(int len)
    {
        var data = new byte[len];
        RandomNumberGenerator.Fill(data);
        return data;
    }
    private static byte[] Sha256(string input)
        => SHA256.HashData(Encoding.ASCII.GetBytes(input));

    private static string Base64Url(byte[] bytes)
        => Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");

    // JSON shape from Google token endpoint
    private record TokenResponse(
        string access_token,
        int expires_in,
        string token_type,
        string? refresh_token,
        string? scope
    );
}
