using Microsoft.Extensions.Options;
using YoutubeDiscogsWantlist.Messaging.Configuration;
using YoutubeDiscogsWantlist.Messaging.Inrastructure;

namespace YoutubeDiscogsWantlist.Analytics.RabbitMq;

/// <summary>
/// Declares the queues/exchanges at application start-up.
/// </summary>
internal sealed class RabbitMqInitializer : IHostedService
{
    private readonly RabbitMqConnectionFactory _factory;
    private readonly RabbitMqOptions _opts;

    public RabbitMqInitializer(RabbitMqConnectionFactory factory,
                               IOptions<RabbitMqOptions> opts)
    {
        _factory = factory;
        _opts = opts.Value;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var channel = await _factory.CreateChanell(ct);

        await channel.QueueDeclareAsync(
            queue: RabbitMqConstants.WantlistAddedQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);

        await channel.BasicQosAsync(0, _opts.PrefetchCount, false, ct);
    }

    public Task StopAsync(CancellationToken _) => Task.CompletedTask;
}
