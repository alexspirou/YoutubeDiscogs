
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using YoutubeDiscogsWantlist.AppUsers;
using YoutubeDiscogsWantlist.Core.WantList;
using YoutubeDiscogsWantlist.Efcore;
using YoutubeDiscogsWantlist.Messaging.Abstractions;
using YoutubeDiscogsWantlist.Messaging.Configuration;
using YoutubeDiscogsWantlist.Messaging.Inrastructure;
using YoutubeDiscogsWantlist.Messaging.Inrastructure.Publisher;
using YoutubeDiscogsWantlist.Services;
using YoutubeDiscogsWantlist.Settings;

namespace YoutubeDiscogsWantlist
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .WithOrigins(
                            "chrome-extension://lfbgiilhhihopjfndcoldhdjbalalhdj",
                            "https://localhost:7076")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
                 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.Configure<DiscogsSettings>(builder.Configuration.GetSection(nameof(DiscogsSettings)));
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                // You can also scan other assemblies if handlers are external
                // cfg.RegisterServicesFromAssembly(typeof(SomeHandler).Assembly);
            });



            builder.Services.AddHttpClient<DiscogsAuthorizationService>((client) =>
            {
                client.BaseAddress = new Uri("https://api.discogs.com/");
                client.DefaultRequestHeaders.Add("Accept", "application/x-www-form-urlencoded");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("DiscogsOAuthClient/1.0 (+https://yourdomain.com)");


            });


            builder.Services.AddHttpClient<DiscogsService>((client) =>
            {
                client.BaseAddress = new Uri("https://api.discogs.com/");
                client.DefaultRequestHeaders.Add("Accept", "application/x-www-form-urlencoded");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("DiscogsOAuthClient/1.0 (+https://yourdomain.com)");
            });

            builder.Services.Configure<RabbitMqOptions>(
            builder.Configuration.GetSection("RabbitMq"));

            // 2. Register the connection factory and publisher
            builder.Services.AddSingleton<RabbitMqConnectionFactory>();
            builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
            builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisherPubSub>();



            builder.Services.AddScoped<IAppUserService, AppUserService>();
            builder.Services.AddScoped<IWantListItemService, WantListItemService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");


            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }

}
