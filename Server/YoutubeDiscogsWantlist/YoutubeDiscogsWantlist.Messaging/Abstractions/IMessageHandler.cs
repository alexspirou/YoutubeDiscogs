namespace YoutubeDiscogsWantlist.Messaging.Abstractions;

public interface IMessageHandler<in T>
{
    Task HandleAsync(T message, CancellationToken ct);
}
