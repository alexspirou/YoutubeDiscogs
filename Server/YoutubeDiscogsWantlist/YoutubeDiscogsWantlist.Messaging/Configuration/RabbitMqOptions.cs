﻿namespace YoutubeDiscogsWantlist.Messaging.Configuration;

public sealed record RabbitMqOptions
{
    public string HostName { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string VirtualHost { get; init; } = "/";
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public bool UseTls { get; init; } = false;
    public ushort PrefetchCount { get; init; } = 10;
}