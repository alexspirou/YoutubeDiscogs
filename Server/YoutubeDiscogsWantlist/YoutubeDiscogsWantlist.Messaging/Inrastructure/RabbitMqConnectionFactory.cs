using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using YoutubeDiscogsWantlist.Messaging.Configuration;

namespace YoutubeDiscogsWantlist.Messaging.Inrastructure;

public sealed class RabbitMqConnectionFactory
{

    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly Lazy<Task<IConnection>> _connection;

    public RabbitMqConnectionFactory(IOptions<RabbitMqOptions> rabbitMqOptions)
    {
        _rabbitMqOptions = rabbitMqOptions.Value;
        _connection = new Lazy<Task<IConnection>>(() => CreateConnection(), LazyThreadSafetyMode.ExecutionAndPublication);
    }


    private Task<IConnection> CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqOptions.HostName,
            Port = _rabbitMqOptions.Port,
            VirtualHost = _rabbitMqOptions.VirtualHost,
            UserName = _rabbitMqOptions.UserName,
            Password = _rabbitMqOptions.Password,
            Ssl = { Enabled = _rabbitMqOptions.UseTls }
        };

        return factory.CreateConnectionAsync();
    }

    public async Task<IChannel> CreateChanell(CancellationToken cancellationToken)
    {
        var connection = await _connection.Value.ConfigureAwait(false);

        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        return channel;
    }
}
