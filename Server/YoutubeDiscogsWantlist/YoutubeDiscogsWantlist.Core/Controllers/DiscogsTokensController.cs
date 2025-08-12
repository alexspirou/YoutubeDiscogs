using Microsoft.AspNetCore.Mvc;
using YoutubeDiscogsWantlist.AppUsers;
using YoutubeDiscogsWantlist.Exceptions;
using YoutubeDiscogsWantlist.Services;

namespace YoutubeDiscogsWantlist.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscogsTokensController : ControllerBase
{
    private readonly DiscogsAuthorizationService _discogsAuthorizationService;
    private readonly IAppUserService _appUserService;

    public DiscogsTokensController(DiscogsAuthorizationService discogsAuthorizationService, IAppUserService appUserService)
    {
        _discogsAuthorizationService = discogsAuthorizationService;
        _appUserService = appUserService;
    }

    /// <summary>
    /// Step 1: Get authorize URL + request token & secret.
    /// </summary>
    [HttpGet("authorize-url")]
    public async Task<IActionResult> GetAuthorizeUrl()
    {
        var result = await _discogsAuthorizationService.GetAuthorizeUrlWithTokenAsync();
        return Ok(new
        {
            authorizeUrl = result.AuthorizeUrl,
            requestToken = result.RequestToken,
            requestTokenSecret = result.RequestTokenSecret
        });
    }

    /// <summary>
    /// Step 3: Exchange request token + secret + verifier for final access token.
    /// </summary>
    [HttpPost("access")]
    public async Task<IActionResult> AccessToken(
        [FromQuery] string oauth_token,
        [FromQuery] string oauth_token_secret,
        [FromQuery] string oauth_verifier)
    {
        var access = await _discogsAuthorizationService.ExchangeAccessTokenAsync(
                         oauth_token,
                         oauth_token_secret,
        oauth_verifier);

        var identity = await _discogsAuthorizationService.GetIdentity(access.OAuthToken, access.OAuthTokenSecret);

        try
        {
            await _appUserService.CreateUser(identity.Username, identity.Id, access.OAuthToken, access.OAuthTokenSecret);
        }
        catch (UserAlreadyExistsException ex) // Todo add error handling pipeline
        {
            return Conflict(new { error = ex.Message });

        }
        return Ok(access);


    }

    [HttpGet("test")]
    public async Task<IActionResult> Test(string token, string secret)
    {
        var identity = await _discogsAuthorizationService.GetIdentity(token, secret);
        return Ok(identity);
    }
}
