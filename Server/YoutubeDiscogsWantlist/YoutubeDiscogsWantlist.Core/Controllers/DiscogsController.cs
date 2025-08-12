using DiscogsYoutube.Shared.Messaging.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using YoutubeDiscogsWantlist.AppUsers;
using YoutubeDiscogsWantlist.Core.WantList;
using YoutubeDiscogsWantlist.Messaging.Abstractions;
using YoutubeDiscogsWantlist.Messaging.Configuration;
using YoutubeDiscogsWantlist.Messaging.Inrastructure.Publisher;
using YoutubeDiscogsWantlist.Services;

namespace YoutubeDiscogsWantlist.Controllers;

[ApiController]
[Route("api/discogs")]
public class DiscogsController : ControllerBase
{
    private readonly DiscogsService _discogsService;
    private readonly IAppUserService _appUserService;
    private readonly DiscogsAuthorizationService _discogsAuthorizationService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMessagePublisher _messagePublisherPubSub;
    private readonly IWantListItemService _wantListItemService;
    public DiscogsController(DiscogsService discogsService, IAppUserService appUserService,
        DiscogsAuthorizationService discogsAuthorizationService, IEnumerable<IMessagePublisher> messagePublishers, IWantListItemService wantListItemService)
    {


        _discogsService = discogsService;
        _wantListItemService = wantListItemService;
        _appUserService = appUserService;
        _discogsAuthorizationService = discogsAuthorizationService;

        _messagePublisher = messagePublishers
             .OfType<RabbitMqPublisher>()
             .First();


        _messagePublisherPubSub = messagePublishers
             .OfType<RabbitMqPublisherPubSub>()
             .First();




    }

    // Search for Discogs releases
    [HttpGet("releases/search")]
    public async Task<IActionResult> SearchReleases([FromQuery] string query, [FromQuery] int page = 1)
    {
        var result = await _discogsService.SearchReleasesAsync(query, page);
        return Ok(result);
    }

    [HttpPost("users/{username}/wantlist/{releaseId}")]
    public async Task<IActionResult> AddToWantlist(
        string username = "alexspyrou",
        int releaseId = 38742)
    {


        // Lookup tokens for this username (unique)
        var user = await _appUserService.GetByUserName(username);
        if (user == null)
            return NotFound(new { error = $"User {username} not found" });
        var identity = await _discogsAuthorizationService.GetIdentity(user.OAuthToken, user.OAuthTokenSecret);



        await _discogsService.AddReleaseToWantlistAsync(
            username,
            releaseId,
            user.OAuthToken,
            user.OAuthTokenSecret
        );

        //await _messagePublisher.PublishAsync(
        //           exchange: "",
        //           routingKey: RabbitMqQueues.WantlistAddedQueue,
        //           message: new UserAddToWantList
        //           {
        //               UserName = username,
        //               ReleaseId = releaseId,
        //               VideoUrl = "some url",
        //               TimestampsUtc = DateTime.UtcNow
        //           });


        await _messagePublisherPubSub.PublishAsync(
                   exchange: RabbitMqConstants.PubSub,
                   routingKey: "",
                   message: new UserAddToWantList
                   {
                       UserName = username,
                       ReleaseId = releaseId,
                       VideoUrl = "some url",
                       TimestampsUtc = DateTime.UtcNow
                   });



        return NoContent();
    }


    [HttpGet("users/{username}/wantlist")]
    public async Task<IActionResult> GetWantList(
        string username,
        int page = 1,
        int perPage = 10)
    {
        // Lookup tokens for this username (unique)
        var user = await _appUserService.GetByUserName(username);
        if (user == null)
            return NotFound(new { error = $"User {username} not found" });

        var identity = await _discogsAuthorizationService.GetIdentity(user.OAuthToken, user.OAuthTokenSecret);
        if (identity.Username != user.DiscogsUsername)
            throw new Exception("Username is not the same");

        // Call the Discogs API (or your service) to get the wantlist
        var response = await _discogsService.GetWantlist(
              username,
              user.OAuthToken,
              user.OAuthTokenSecret, page, perPage
          );

        // Serialize the response to JSON (for ETag generation)
        string responseJson = JsonSerializer.Serialize(response);

        // Generate a weak ETag using a hash of the response
        string etag = $"W/\"{Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(responseJson)))}\"";

        // Check If-None-Match from client
        if (Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch)
            && ifNoneMatch == etag)
        {
            // Not modified, return 304
            return StatusCode(StatusCodes.Status304NotModified);
        }

        // Set ETag header
        Response.Headers["ETag"] = etag;
        Response.Headers["Cache-Control"] = "public,max-age=60"; // Cache for 1 minute

        return Ok(response);
    }


    [HttpGet("users/{username}/wantlist-db")]
    public async Task<IActionResult> GetWantListFromDb(
    string username,
    int page = 1,
    int perPage = 10)
    {
        // Lookup tokens for this username (unique)
        var user = await _appUserService.GetByUserName(username);
        if (user == null)
            return NotFound(new { error = $"User {username} not found" });

        var identity = await _discogsAuthorizationService.GetIdentity(user.OAuthToken, user.OAuthTokenSecret);
        if (identity.Username != user.DiscogsUsername)
            throw new Exception("Username is not the same");

        var response = await _wantListItemService.GetWantListByUserName(username);


        // Generate a weak ETag using a hash of the response
        string etag = $"W/\"{Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(response)))}\"";

        // Check If-None-Match from client
        if (Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch)
            && ifNoneMatch == etag)
        {
            // Not modified, return 304
            return StatusCode(StatusCodes.Status304NotModified);
        }

        // Set ETag header
        Response.Headers["ETag"] = etag;
        Response.Headers["Cache-Control"] = "public,max-age=60"; // Cache for 1 minute

        return Ok(response);
    }






    // TEST Endpoints


    //[HttpGet("users/{username}/wantlistAndStoreDb")]
    //public async Task<IActionResult> GetWantlistAndStoreDb(
    //string username,
    //int page = 1,
    //int perPage = 10)
    //{
    //    // Lookup tokens for this username (unique)
    //    var user = await _appUserService.GetByUserName(username);
    //    if (user == null)
    //        return NotFound(new { error = $"User {username} not found" });

    //    var identity = await _discogsAuthorizationService.GetIdentity(user.OAuthToken, user.OAuthTokenSecret);
    //    if (identity.Username != user.DiscogsUsername)
    //        throw new Exception("Username is not the same");

    //    // Call the Discogs API (or your service) to get the wantlist
    //    var response = await _discogsService.GetWantlist(
    //          username,
    //          user.OAuthToken,
    //          user.OAuthTokenSecret, page, perPage
    //      );


    //    var wantList = response.Wants.Select(x =>
    //     new WantListItem
    //     {
    //         UserName = identity.Username,
    //         Artist = x.BasicInformation.Artists.FirstOrDefault().Name ?? "N/A",
    //         Label = x.BasicInformation.Labels.FirstOrDefault().Name ?? "N/A",
    //         Title = x.BasicInformation.Title,
    //         Year = x.BasicInformation.Year,

    //     }).ToList();


    //    await _wantListItemService.AddWantListItems(wantList);

    //    return Ok();
    //}




    //[HttpGet("users/{username}/wantlist-yield")]
    //public async Task<IActionResult> GetWantListWithYield(
    //string username,
    //int page = 1,
    //int perPage = 10)
    //{


    //    // Lookup tokens for this username (unique)
    //    var user = await _appUserService.GetByUserName(username);
    //    if (user == null)
    //        return NotFound(new { error = $"User {username} not found" });
    //    var identity = await _discogsAuthorizationService.GetIdentity(user.OAuthToken, user.OAuthTokenSecret);

    //    if (identity.Username != user.DiscogsUsername)
    //    {
    //        throw new Exception("Username is not the same");
    //    }

    //    var response = _discogsService.GetWantlistWithYield(
    //          username,
    //          user.OAuthToken,
    //          user.OAuthTokenSecret
    //    );

    //    await foreach (var want in response)
    //    {
    //        Console.WriteLine($"Want: {want.BasicInformation.Title}");
    //    }

    //    return Ok(response);
    //}


}
