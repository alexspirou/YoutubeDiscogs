namespace YoutubeDiscogsWantlist.Messaging.Abstractions;


public interface IMessagePublisher
{
    Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken ct = default);
}