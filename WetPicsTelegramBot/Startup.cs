using System;
using System.IO;
using Imouto.BooruParser.Loaders;
using IqdbApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using Quartz.Spi;
using Telegram.Bot;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Database.Context;
using WetPicsTelegramBot.Helpers;
using WetPicsTelegramBot.Jobs;
using WetPicsTelegramBot.Services;
using WetPicsTelegramBot.Services.Abstract;
using WetPicsTelegramBot.Services.Dialog;

namespace WetPicsTelegramBot
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"config/AppSettings.{env.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            Configuration = configuration;
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection serviceCollection, IHostingEnvironment env)
        {
            // logger
            var configFileRelativePath = $"config/nlog.{env.EnvironmentName}.config";

            var nlog = new LoggerFactory().AddNLog();
            nlog.ConfigureNLog(configFileRelativePath);

            serviceCollection.AddSingleton(nlog);
            serviceCollection.AddLogging();

            // configuration
            
            serviceCollection.AddOptions();
            serviceCollection.Configure<AppSettings>(Configuration.GetSection("Configuration"));
            serviceCollection.AddTransient<AppSettings>(services => services.GetService<IOptions<AppSettings>>().Value);

            // services

            serviceCollection.AddTransient<IJobFactory, InjectableJobFactory>();
            serviceCollection.AddTransient<PostDayTopJob>();

            serviceCollection.AddTransient<ITopRatingService, TopRatingService>();
            serviceCollection.AddTransient<IUserTrackingService, UserTrackingService>();
            serviceCollection.AddTransient<ISchedulerService, SchedulerService>();

            serviceCollection.AddSingleton<ITelegramBotClient>(CreateTelegramBotClient);
            serviceCollection.AddSingleton<IIqdbClient, IqdbClient>();
            serviceCollection.AddSingleton<IRepostSettingsService, RepostSettingsService>();
            serviceCollection.AddSingleton<IPixivSettingsService, PixivSettingsService>();
            serviceCollection.AddSingleton<IScheduledResultsService, ScheduledResultsService>();

            serviceCollection.AddTransient<IDbRepository, DbRepository>();
            serviceCollection.AddTransient<IPixivRepository, PixivRepository>();

            serviceCollection.AddSingleton<PixivService>();
            serviceCollection.AddSingleton<IImageRepostService, ImageRepostService>();
            serviceCollection.AddSingleton<IForwardService, ForwardService>();

            serviceCollection.AddSingleton<IMessagesObservableService, MessagesObservableService>();
            serviceCollection.AddSingleton<IDialogObserverService, DialogObserverService>();
            serviceCollection.AddSingleton<IIqdbService, IqdbService>();
            serviceCollection.AddSingleton<ITopImageDrawService, TopImageDrawService>();

            serviceCollection.AddSingleton<IMessagesService, MessagesService>();
            serviceCollection.AddSingleton<ICommandsService, CommandsService>();
            serviceCollection.AddSingleton<IDialogServiceInitializer, DialogServiceInitializer>();

            serviceCollection.AddSingleton<IDialogService<HelpDialogService>, HelpDialogService>();
            serviceCollection.AddSingleton<IDialogService<RepostDialogService>, RepostDialogService>();
            serviceCollection.AddSingleton<IDialogService<StatsDialogService>, StatsDialogService>();
            serviceCollection.AddSingleton<IDialogService<PixivDialogService>, PixivDialogService>();
            serviceCollection.AddSingleton<IDialogService<TopDialogService>, TopDialogService>();
            serviceCollection.AddSingleton<IDialogService<IqdbDialogService>, IqdbDialogService>();

            serviceCollection.AddSingleton<DanbooruLoader>(CreateDanbooruLoader);
            serviceCollection.AddSingleton<SankakuLoader>(CreateSankakuLoader);
            serviceCollection.AddSingleton<YandereLoader>();

            serviceCollection.AddDbContext<WetPicsDbContext>((serviceProvider, optionBuilder) =>
            {
                var connectionString = serviceProvider.GetService<IOptions<AppSettings>>().Value.ConnectionString;
                optionBuilder.UseNpgsql(connectionString);
            }, ServiceLifetime.Transient);

            // app
            serviceCollection.AddSingleton<App>();
        }

        private SankakuLoader CreateSankakuLoader(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<AppSettings>().SankakuConfiguration;

            return new SankakuLoader(config.Login, config.PassHash, config.Delay);
        }

        private DanbooruLoader CreateDanbooruLoader(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<AppSettings>().DanbooruConfiguration;

            return new DanbooruLoader(config.Login, config.ApiKey, config.Delay);
        }

        private ITelegramBotClient CreateTelegramBotClient(IServiceProvider serviceProvider)
        {
            var token = serviceProvider.GetService<AppSettings>().BotToken;

            var telegramBotClient = new TelegramBotClient(token);
            return telegramBotClient;
        }
    }
}