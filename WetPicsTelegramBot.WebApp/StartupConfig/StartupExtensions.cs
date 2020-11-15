using System;
using System.Linq;
using System.Net.Http;
using Imouto.BooruParser.Loaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.QueuedWrapper;
using WetPicsTelegramBot.Data.Context;
using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.WebApp.Models.Settings;
using WetPicsTelegramBot.WebApp.Services;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.StartupConfig
{
    public static class StartupExtensions
    {
        public static IApplicationBuilder UseWetPicsDbContext(this IApplicationBuilder app)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<Program>>();

            try
            {
                using (var serviceScope = app.ApplicationServices.CreateScope())
                using (var db = serviceScope.ServiceProvider.GetRequiredService<WetPicsDbContext>())
                {
                    db.Database.Migrate();

                    var botId = int.Parse(
                        app.ApplicationServices.GetRequiredService<AppSettings>().BotToken.Split(':').First());

                    var botUser = db.ChatUsers.FirstOrDefault(x => x.UserId == botId);

                    if (botUser == null)
                    {
                        var chatUser = new ChatUser {FirstName = "WetPicsBot", UserId = botId};

                        db.ChatUsers.Add(chatUser);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occurred in migration process");
                throw;
            }

            return app;
        }

        public static IServiceCollection AddSettings<T>(this IServiceCollection services, IConfiguration configuration)
            where T : class, new()
        {
            services.AddOptions();
            services.Configure<T>(configuration.GetSection("Configuration"));
            services.AddTransient(ser => ser.GetRequiredService<IOptions<T>>().Value);

            return services;
        }

        public static IApplicationBuilder UseQuartz(this IApplicationBuilder applicationBuilder)
        {
            var lifetime = applicationBuilder.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
            var serviceScopeFactory = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>();

            var quartz = new QuartzStartup(serviceScopeFactory);

            lifetime.ApplicationStarted.Register(quartz.Start);
            lifetime.ApplicationStopping.Register(quartz.Stop);

            return applicationBuilder;
        }

        public static IApplicationBuilder UseTelegramBotWebhook(this IApplicationBuilder applicationBuilder)
        {
            var services = applicationBuilder.ApplicationServices;

            var lifetime = services.GetRequiredService<IHostApplicationLifetime>();

            lifetime.ApplicationStarted.Register(
                () =>
                {
                    var logger = services.GetRequiredService<ILogger<Startup>>();


                    var address = services.GetRequiredService<AppSettings>().WebHookAddress;
                    var telegramBotClient = services.GetRequiredService<ITelegramBotClient>();

                    logger.LogInformation("Removing webhook");
                    telegramBotClient.DeleteWebhookAsync().Wait();

                    logger.LogInformation($"Setting webhook to {address}");
                    telegramBotClient.SetWebhookAsync(address, maxConnections: 5).Wait();
                    logger.LogInformation($"Webhook is set to {address}");

                    var webhookInfo = telegramBotClient.GetWebhookInfoAsync().Result;
                    logger.LogInformation($"Webhook info: {JsonConvert.SerializeObject(webhookInfo)}");
                });

            lifetime.ApplicationStopping.Register(
                () =>
                {
                    var logger = services.GetService<ILogger<Startup>>();

                    services.GetRequiredService<ITelegramBotClient>().DeleteWebhookAsync().Wait();
                    logger.LogInformation("Webhook removed");
                });

            return applicationBuilder;
        }

        public static IServiceCollection AddBooruLoaders(this IServiceCollection services)
        {
            services.AddTransient(CreateDanbooruLoader);
            services.AddTransient(CreateSankakuLoader);
            services.AddTransient<YandereLoader>();

            return services;
        }

        private static SankakuLoader CreateSankakuLoader(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetRequiredService<AppSettings>().SankakuConfiguration;

            return new SankakuLoader(config.Login, config.PassHash, config.Delay);
        }

        private static DanbooruLoader CreateDanbooruLoader(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetRequiredService<AppSettings>().DanbooruConfiguration;

            return new DanbooruLoader(config.Login, config.ApiKey, config.Delay);
        }

        public static IServiceCollection AddTelegramClient(this IServiceCollection services)
        {
            services.AddTransient<ITgClient, TgClient>();
            services.AddTransient(CreateTelegramBotClient);

            return services;
        }

        private static ITelegramBotClient CreateTelegramBotClient(IServiceProvider serviceProvider)
        {
            var token = serviceProvider.GetRequiredService<AppSettings>().BotToken;
            var httpClient = serviceProvider.GetService<HttpClient>();

            var telegramBotClient = new QueuedTelegramBotClient(token, httpClient);
            return telegramBotClient;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            services.AddDbContext<WetPicsDbContext>(
                (serviceProvider, optionBuilder) =>
                {
                    var connectionString = serviceProvider.GetRequiredService<AppSettings>().ConnectionString;
                    optionBuilder.UseNpgsql(connectionString);
                },
                ServiceLifetime.Transient);

            return services;
        }
    }
}
