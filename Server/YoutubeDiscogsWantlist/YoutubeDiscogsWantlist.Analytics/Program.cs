namespace YoutubeDiscogsWantlist.Analytics;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddRabbitMqInfrastructure(builder.Configuration);

        var host = builder.Build();
        host.Run();
    }
}