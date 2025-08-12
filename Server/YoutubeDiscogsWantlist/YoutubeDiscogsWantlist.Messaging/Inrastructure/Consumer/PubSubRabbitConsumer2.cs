using DiscogsYoutube.Shared.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using YoutubeDiscogsWantlist.Messaging.Abstractions;

namespace YoutubeDiscogsWantlist.Messaging.Inrastructure.Consumer;

public class PubSubRabbitConsumer2<TEvent> : BackgroundService
{
    protected readonly string _exchange;
    protected readonly RabbitMqConnectionFactory _rabbitMqConnectionFactory;
    protected readonly IMessageHandler<TEvent> _handler;
    protected readonly ILogger<BaseRabbitConsumer<TEvent>> _log;
    protected readonly string _queue;
    protected readonly ushort _prefetch;
    protected IChannel _channel;
    protected CancellationToken _cancellationToken;
    public PubSubRabbitConsumer2(RabbitMqConnectionFactory rabbitMqConnectionFactory,
        IMessageHandler<TEvent> handler, ILogger<BaseRabbitConsumer<TEvent>> log, string queueName, string exchange)

    {
        _rabbitMqConnectionFactory = rabbitMqConnectionFactory;
        _handler = handler;
        _log = log;
        _queue = queueName;
        _exchange = exchange;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            _channel ??= await _rabbitMqConnectionFactory.CreateChanell(cancellationToken);

            await _channel.ExchangeDeclareAsync(exchange: _exchange, type: ExchangeType.Fanout, durable: true);
            await _channel.QueueDeclareAsync(queue: _queue, durable: true, exclusive: false, autoDelete: false);

            await _channel.QueueBindAsync(queue: _queue, exchange: _exchange, routingKey: "");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnMessageReceived;
            await _channel.BasicConsumeAsync(
                    queue: _queue,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: cancellationToken
                );
            await Task.Delay(Timeout.Infinite, cancellationToken);

        }
        catch (Exception ex)
        {

        }



    }


    protected virtual async Task OnMessageReceived(object sender, BasicDeliverEventArgs @event)
    {

        try
        {
            var msg = SystemTextJsonSerializer.Deserialize<TEvent>(@event.Body.Span)!;
            await _handler.HandleAsync(msg, _cancellationToken);
            await _channel.BasicAckAsync(@event.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed handling message from {Queue}", _queue);
            await _channel.BasicNackAsync(@event.DeliveryTag, false, requeue: false);
        }

    }
}
