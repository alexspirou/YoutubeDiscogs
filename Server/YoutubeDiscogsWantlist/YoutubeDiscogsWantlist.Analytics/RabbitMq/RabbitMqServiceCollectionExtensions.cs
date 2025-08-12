namespace YoutubeDiscogsWantlist.Analytics;

using DiscogsYoutube.Shared.Messaging.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YoutubeDiscogsWantlist.Analytics.Handlers;
using YoutubeDiscogsWantlist.Analytics.RabbitMq;
using YoutubeDiscogsWantlist.Messaging.Abstractions;
using YoutubeDiscogsWantlist.Messaging.Configuration;
using YoutubeDiscogsWantlist.Messaging.Inrastructure;
using YoutubeDiscogsWantlist.Messaging.Inrastructure.Consumer;
using YoutubeDiscogsWantlist.Messaging.Inrastructure.Publisher;

public static class RabbitMqServiceCollectionExtensions
{

    public static IServiceCollection AddRabbitMqInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options + core infrastructure
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

        // Message handlers
        services.AddSingleton<IMessageHandler<UserAddToWantList>, UserAddToWantListHandler>();
        services.AddSingleton<IMessageHandler<UserAddToWantList>, UserAddToWantListHandlerPubSub>();

        // Startup tasks
        services.AddHostedService<RabbitMqInitializer>();                         // declares queues
        services.AddHostedService(provider =>
        {
            var factory = provider.GetRequiredService<RabbitMqConnectionFactory>();
            var handler = provider.GetRequiredService<IMessageHandler<UserAddToWantList>>();
            var log = provider.GetRequiredService<ILogger<
                               BaseRabbitConsumer<UserAddToWantList>>>();

            return new BaseRabbitConsumer<UserAddToWantList>(
                factory, handler, log, RabbitMqConstants.WantlistAddedQueue);
        });


        services.AddHostedService(provider =>
        {
            var factory = provider.GetRequiredService<RabbitMqConnectionFactory>();
            var handlers = provider.GetRequiredService<IEnumerable<IMessageHandler<UserAddToWantList>>>();
            var log = provider.GetRequiredService<ILogger<
                               BaseRabbitConsumer<UserAddToWantList>>>();
            var selectedHandler = handlers
            .OfType<UserAddToWantListHandlerPubSub>()
            .First();
            return new PubSubRabbitConsumer<UserAddToWantList>(
                factory, selectedHandler, log, "consumer-queue1", RabbitMqConstants.PubSub);
        });


        services.AddHostedService(provider =>
        {
            var factory = provider.GetRequiredService<RabbitMqConnectionFactory>();
            var handlers = provider.GetRequiredService<IEnumerable<IMessageHandler<UserAddToWantList>>>();
            var log = provider.GetRequiredService<ILogger<
                               BaseRabbitConsumer<UserAddToWantList>>>();
            var selectedHandler = handlers
            .OfType<UserAddToWantListHandlerPubSub>()
            .First();
            return new PubSubRabbitConsumer2<UserAddToWantList>(
                factory, selectedHandler, log, "consumer-queue2", RabbitMqConstants.PubSub);
        });



        //services.AddHostedService(provider =>
        //{
        //    var factory = provider.GetRequiredService<RabbitMqConnectionFactory>();
        //    var handlers = provider.GetRequiredService<IEnumerable<IMessageHandler<UserAddToWantList>>>();
        //    var log = provider.GetRequiredService<ILogger<
        //                       BaseRabbitConsumer<UserAddToWantList>>>();

        //    var selectedHandler = handlers
        //    .OfType<UserAddToWantListHandlerPubSub>()
        //    .First();

        //    return new BaseRabbitConsumer<UserAddToWantList>(
        //        factory, selectedHandler, log, RabbitMqConstants.WantlistAddedQueue);
        //});

        return services;
    }
}
