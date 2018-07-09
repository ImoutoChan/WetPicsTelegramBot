using System;
using System.Linq;
using System.Net.Http;
using Imouto.BooruParser.Loaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.QueuedWrapper;
using WetPicsTelegramBot.Data.Context;
using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.WebApp.Services;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Startup
{
    public static class StartupExtensions
    {
        public static IApplicationBuilder UseNlog(this IApplicationBuilder app, string environmentName = "Development")
        {
            var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();

            var configFileRelativePath = $"config/nlog.{environmentName}.config";

            loggerFactory.AddNLog();
            loggerFactory.ConfigureNLog(configFileRelativePath);

            return app;
        }

        public static IApplicationBuilder UseWetPicsDbContext(this IApplicationBuilder app)
        {
            var logger = app.ApplicationServices.GetService<ILogger<Program>>();

            try
            {
                using (var serviceScope = app.ApplicationServices.CreateScope())
                using (var db = serviceScope.ServiceProvider.GetService<WetPicsDbContext>())
                {
                    db.Database.Migrate();

                    var botId = Int32.Parse(app.ApplicationServices.GetService<AppSettings>().BotToken.Split(':').First());

                    var botUser = db.ChatUsers.FirstOrDefault(x => x.UserId == botId);

                    if (botUser == null)
                    {
                        var chatuser = new ChatUser { FirstName = "WetPicsBot", UserId = botId };

                        db.ChatUsers.Add(chatuser);
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
            services.AddTransient<T>(ser => ser.GetService<IOptions<T>>().Value);

            return services;
        }

        public static IApplicationBuilder UseQuartz(this IApplicationBuilder applicationBuilder)
        {
            var lifetime = applicationBuilder.ApplicationServices.GetService<IApplicationLifetime>();
            var serviceScopeFactory = applicationBuilder.ApplicationServices.GetService<IServiceScopeFactory>();

            var quartz = new QuartzStartup(serviceScopeFactory);

            lifetime.ApplicationStarted.Register(quartz.Start);
            lifetime.ApplicationStopping.Register(quartz.Stop);

            return applicationBuilder;
        }

        public static IApplicationBuilder UseTelegramBotWebhook(this IApplicationBuilder applicationBuilder)
        {
            var services = applicationBuilder.ApplicationServices;

            var lifetime = services.GetService<IApplicationLifetime>();

            lifetime.ApplicationStarted.Register(() =>
            {
                var logger = services.GetService<ILogger<Startup>>();


                var adress = services.GetService<AppSettings>().WebHookAdress;

                logger.LogInformation($"Removind webhook");
                services.GetService<ITelegramBotClient>().DeleteWebhookAsync().Wait();

                logger.LogInformation($"Setting webhook to {adress}");
                services.GetService<ITelegramBotClient>().SetWebhookAsync(adress, maxConnections: 5).Wait();
                logger.LogInformation($"Webhook is set to {adress}");

                var webhookInfo = services.GetService<ITelegramBotClient>().GetWebhookInfoAsync().Result;
                logger.LogInformation($"Webhook info: {JsonConvert.SerializeObject(webhookInfo)}");
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                var logger = services.GetService<ILogger<Startup>>();

                services.GetService<ITelegramBotClient>().DeleteWebhookAsync().Wait();
                logger.LogInformation("Webhook removed");
            });

            return applicationBuilder;
        }

        public static IServiceCollection AddBooruLoaders(this IServiceCollection services)
        {
            services.AddTransient<DanbooruLoader>(CreateDanbooruLoader);
            services.AddTransient<SankakuLoader>(CreateSankakuLoader);
            services.AddTransient<YandereLoader>();

            return services;
        }

        private static SankakuLoader CreateSankakuLoader(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<AppSettings>().SankakuConfiguration;

            return new SankakuLoader(config.Login, config.PassHash, config.Delay);
        }

        private static DanbooruLoader CreateDanbooruLoader(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<AppSettings>().DanbooruConfiguration;

            return new DanbooruLoader(config.Login, config.ApiKey, config.Delay);
        }

        public static IServiceCollection AddTelegramClient(this IServiceCollection services)
        {

            services.AddTransient<ITgClient, TgClient>();
            services.AddTransient<ITelegramBotClient>(CreateTelegramBotClient);

            return services;
        }

        private static ITelegramBotClient CreateTelegramBotClient(IServiceProvider serviceProvider)
        {
            var token = serviceProvider.GetService<AppSettings>().BotToken;
            var httpClient = serviceProvider.GetService<HttpClient>();

            var telegramBotClient = new QueuedTelegramBotClient(token, httpClient);
            return telegramBotClient;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            services.AddDbContext<WetPicsDbContext>((serviceProvider, optionBuilder) =>
            {
                var connectionString = serviceProvider.GetService<AppSettings>().ConnectionString;
                optionBuilder.UseNpgsql(connectionString);
            }, ServiceLifetime.Transient);

            return services;
        }
    }
}