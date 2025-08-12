using DiscogsYoutube.Shared.Messaging.Contracts;
using YoutubeDiscogsWantlist.Messaging.Abstractions;

internal class UserAddToWantListHandlerPubSub : IMessageHandler<UserAddToWantList>
{
    public Task HandleAsync(UserAddToWantList message, CancellationToken ct)
    {

        Console.WriteLine($"{message.UserName} added {message.ReleaseId} with pub sub");

        return Task.CompletedTask;
    }
}