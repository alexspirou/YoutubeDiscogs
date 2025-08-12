using DiscogsYoutube.Shared.Messaging.Contracts;
using YoutubeDiscogsWantlist.Messaging.Abstractions;
namespace YoutubeDiscogsWantlist.Analytics.Handlers;

public class UserAddToWantListHandler : IMessageHandler<UserAddToWantList>
{
    public Task HandleAsync(UserAddToWantList message, CancellationToken ct)
    {

        Console.WriteLine($"{message.UserName} added {message.ReleaseId}");

        return Task.CompletedTask;
    }
}
