using DiscogsYoutube.Shared.Serialization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using YoutubeDiscogsWantlist.Messaging.Abstractions;

namespace YoutubeDiscogsWantlist.Messaging.Inrastructure.Publisher;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{

    protected readonly RabbitMqConnectionFactory _rabbitMqFactory;
    protected readonly ILogger<RabbitMqPublisher> _logger;
    protected IChannel? _channel;

    public RabbitMqPublisher(RabbitMqConnectionFactory rabbitMqConnectionFactory, ILogger<RabbitMqPublisher> logger)
    {
        _rabbitMqFactory = rabbitMqConnectionFactory;
        _logger = logger;
    }


    protected Task<IChannel> CreateChannel(CancellationToken cancellationToken)
    {
        return _rabbitMqFactory.CreateChanell(cancellationToken);
    }



    public virtual async Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default)
    {

        _channel ??= await CreateChannel(cancellationToken);

        var body = Encoding.UTF8.GetBytes(SystemTextJsonSerializer.Serialize(message));

        //await _channel.BasicPublishAsync(exchange: exchange, routingKey: routingKey, body: body, cancellationToken: ct);
        await _channel.BasicPublishAsync("", routingKey, body: body);

        _logger.LogDebug("Published {Type} to {Exchange}:{RoutingKey}", typeof(T).Name, exchange, routingKey);
    }

    public void Dispose() => _channel?.Dispose();


}
