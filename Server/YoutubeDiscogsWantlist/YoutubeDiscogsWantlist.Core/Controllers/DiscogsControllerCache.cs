using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using YoutubeDiscogsWantlist.AppUsers;
using YoutubeDiscogsWantlist.Core.WantList;
using YoutubeDiscogsWantlist.Efcore.Modelsl;
using YoutubeDiscogsWantlist.MediatR.Responses;
using YoutubeDiscogsWantlist.Services;

namespace YoutubeDiscogsWantlist.Core.Controllers;

[ApiController]
[Route("api/discogs-cache")]
public class DiscogsControllerCache : ControllerBase
{
    private readonly DiscogsService _discogsService;
    private readonly IAppUserService _appUserService;
    private readonly DiscogsAuthorizationService _discogsAuthorizationService;
    private readonly IWantListItemService _wantListItemService;

    public DiscogsControllerCache(
        DiscogsService discogsService,
        IAppUserService appUserService,
        DiscogsAuthorizationService discogsAuthorizationService,
        IWantListItemService wantListItemService)
    {
        _discogsService = discogsService;
        _wantListItemService = wantListItemService;
        _appUserService = appUserService;
        _discogsAuthorizationService = discogsAuthorizationService;
    }

    // ----------------------------------------------
    // 1. ETag-based caching
    // Efficient, content-based validation.
    // ----------------------------------------------
    [HttpGet("users/{username}/wantlist-etag")]
    public async Task<IActionResult> GetWantListWithEtag(string username, int page = 1, int perPage = 10)
    {
        // 1. Retrieve user, identity, and wantlist data
        var (user, identity, response) = await GetUserIdentityAndResponse(username);
        if (user == null)
            return NotFound(new { error = $"User {username} not found" });

        // 2. Generate ETag based on the response content
        string etag = $"W/\"{Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(response.ToList())))}\"";

        // 3. Check if client sent If-None-Match with the same ETag
        if (Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch) && ifNoneMatch == etag)
        {
            // 4. If match, return 304 Not Modified (client uses its cache)
            return StatusCode(StatusCodes.Status304NotModified);
        }

        // 5. Otherwise, send ETag and Cache-Control headers with response
        Response.Headers["ETag"] = etag;
        Response.Headers["Cache-Control"] = "public,max-age=60"; // 1 minute

        // 6. Return the actual data
        return Ok(response);
    }

    // ----------------------------------------------
    // 2. Last-Modified header
    // Timestamp-based validation (simpler than ETag).
    // ----------------------------------------------
    [HttpGet("users/{username}/wantlist-lastmodified")]
    public async Task<IActionResult> GetWantListWithLastModified(string username, int page = 1, int perPage = 10)
    {
        // 1. Retrieve user, identity, and wantlist data
        var (user, identity, response) = await GetUserIdentityAndResponse(username);
        if (user == null)
            return NotFound(new { error = $"User {username} not found" });

        // 2. Get (or simulate) the last modified time for this user's wantlist
        DateTime lastModified = DateTime.UtcNow.AddMinutes(-10);

        // 3. If client sent If-Modified-Since header
        if (Request.Headers.TryGetValue("If-Modified-Since", out var ifModifiedSinceStr)
            && DateTime.TryParse(ifModifiedSinceStr, out var ifModifiedSince)
            && lastModified <= ifModifiedSince)
        {
            // 4. If the data wasn't updated since the client's last request, return 304
            return StatusCode(StatusCodes.Status304NotModified);
        }

        // 5. Otherwise, send Last-Modified and Cache-Control headers with response
        Response.Headers["Last-Modified"] = lastModified.ToString("R");
        Response.Headers["Cache-Control"] = "public,max-age=60"; // 1 minute

        // 6. Return the data
        return Ok(response);
    }

    // ----------------------------------------------
    // 3. No-Cache
    // Explicitly disables caching.
    // ----------------------------------------------
    [HttpGet("users/{username}/wantlist-nocache")]
    public async Task<IActionResult> GetWantListNoCache(string username, int page = 1, int perPage = 10)
    {
        // 1. Retrieve user, identity, and wantlist data
        var (user, identity, response) = await GetUserIdentityAndResponse(username);
        if (user == null)
            return NotFound(new { error = $"User {username} not found" });

        // 2. Set HTTP headers to forbid any caching
        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        // 3. Return the data
        return Ok(response);
    }

    // ----------------------------------------------
    // 4. Expires header
    // Allows caching until a specific point in time.
    // ----------------------------------------------
    [HttpGet("users/{username}/wantlist-expires")]
    public async Task<IActionResult> GetWantListWithExpires(string username, int page = 1, int perPage = 10)
    {
        // 1. Retrieve user, identity, and wantlist data
        var (user, identity, response) = await GetUserIdentityAndResponse(username);
        if (user == null)
            return NotFound(new { error = $"User {username} not found" });

        // 2. Set an absolute expiry time (1 minute from now)
        Response.Headers["Expires"] = DateTime.UtcNow.AddMinutes(1).ToString("R");
        Response.Headers["Cache-Control"] = "public";

        // 3. Return the data
        return Ok(response);
    }

    // ----------------------------------------------
    // 5. [ResponseCache] attribute
    // Simplest way to add cache headers in ASP.NET Core.
    // ----------------------------------------------
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, NoStore = false)]
    [HttpGet("users/{username}/wantlist-responsecache")]
    public async Task<IActionResult> GetWantListWithResponseCache(string username, int page = 1, int perPage = 10)
    {
        // 1. Retrieve user, identity, and wantlist data
        var (user, identity, response) = await GetUserIdentityAndResponse(username);
        if (user == null)
            return NotFound(new { error = $"User {username} not found" });

        // 2. [ResponseCache] automatically sets Cache-Control and other headers

        // 3. Return the data
        return Ok(response);
    }

    // ----------------------------------------------
    // Private helper: Gets user, identity, and wantlist as a tuple.
    // Throws if identity does not match (your original logic).
    // ----------------------------------------------
    private async Task<(DiscogsUser user, DiscogsIdentityResponse? identity, IList<WantListItem> response)> GetUserIdentityAndResponse(string username)
    {
        // 1. Get user by username
        var user = await _appUserService.GetByUserName(username);
        if (user == null)
            return (null, null, null);

        // 2. Get Discogs identity using OAuth tokens
        var identity = await _discogsAuthorizationService.GetIdentity(user.OAuthToken, user.OAuthTokenSecret);
        if (identity.Username != user.DiscogsUsername)
            throw new Exception("Username does not match.");

        // 3. Get wantlist data (as string)
        var response = await _wantListItemService.GetWantListByUserName(username);

        // 4. Return tuple
        return (user, identity, response);
    }
}
