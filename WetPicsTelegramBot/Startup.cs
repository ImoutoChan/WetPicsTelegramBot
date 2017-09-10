using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using Telegram.Bot;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Database.Context;
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
            serviceCollection.AddSingleton<ITelegramBotClient>(CreateTelegramBotClient);
            serviceCollection.AddSingleton<IRepostSettingsService, RepostSettingsService>();
            serviceCollection.AddSingleton<IPixivSettingsService, PixivSettingsService>();

            serviceCollection.AddTransient<IDbRepository, DbRepository>();
            serviceCollection.AddTransient<IPixivRepository, PixivRepository>();
            
            serviceCollection.AddSingleton<PixivService>();
            serviceCollection.AddSingleton<IImageRepostService, ImageRepostService>();

            serviceCollection.AddSingleton<IMessagesObservableService, MessagesObservableService>();
            serviceCollection.AddSingleton<IDialogObserverService, DialogObserverService>();

            serviceCollection.AddSingleton<IMessagesService, MessagesService>();
            serviceCollection.AddSingleton<ICommandsService, CommandsService>();
            serviceCollection.AddSingleton<IDialogServiceInitializer, DialogServiceInitializer>();

            serviceCollection.AddSingleton<IDialogService<HelpDialogService>, HelpDialogService>();
            serviceCollection.AddSingleton<IDialogService<RepostDialogService>, RepostDialogService>();
            serviceCollection.AddSingleton<IDialogService<StatsDialogService>, StatsDialogService>();
            serviceCollection.AddSingleton<IDialogService<PixivDialogService>, PixivDialogService>();
            serviceCollection.AddSingleton<IDialogService<TopDialogService>, TopDialogService>();

            serviceCollection.AddDbContext<WetPicsDbContext>((serviceProvider, optionBuilder) =>
            {
                var connectionString = serviceProvider.GetService<IOptions<AppSettings>>().Value.ConnectionString;
                optionBuilder.UseNpgsql(connectionString);
            }, ServiceLifetime.Transient);

            // app
            serviceCollection.AddSingleton<App>();
        }

        private ITelegramBotClient CreateTelegramBotClient(IServiceProvider serviceProvider)
        {
            var token = serviceProvider.GetService<AppSettings>().BotToken;

            var telegramBotClient = new TelegramBotClient(token);
            return telegramBotClient;
        }
    }
}