using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using NLog.Targets;
using Telegram.Bot;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Database.Model;
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
                .AddJsonFile($"AppSettings.{env.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            Configuration = configuration;
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            // logger
            serviceCollection.AddSingleton(new LoggerFactory().AddNLog());
            serviceCollection.AddLogging();
            SetupLogger();

            // configuration
            
            serviceCollection.AddOptions();
            serviceCollection.Configure<AppSettings>(Configuration.GetSection("Configuration"));

            // services
            serviceCollection.AddSingleton<ITelegramBotClient>(CreateTelegramBotClient);
            serviceCollection.AddSingleton<IChatSettings, ChatSettings>();
            serviceCollection.AddSingleton<IPixivSettings, PixivSettings>();

            serviceCollection.AddTransient<IDbRepository, DbRepository>();
            serviceCollection.AddTransient<IPixivRepository, PixivRepository>();
            
            serviceCollection.AddSingleton<PixivService>();
            serviceCollection.AddSingleton<PhotoPublisherService>();
            serviceCollection.AddSingleton<IDialogObserverService, DialogObserverService>();
            serviceCollection.AddSingleton<IMessagesService, MessagesService>();
            serviceCollection.AddSingleton<ICommandsService, CommandsService>();
            serviceCollection.AddSingleton<IDialogServiceInitializer, DialogServiceInitializer>();

            serviceCollection.AddSingleton<IDialogService<HelpDialogService>, HelpDialogService>();
            serviceCollection.AddSingleton<IDialogService<RepostDialogService>, RepostDialogService>();
            serviceCollection.AddSingleton<IDialogService<StatsDialogService>, StatsDialogService>();
            serviceCollection.AddSingleton<IDialogService<PixivDialogService>, PixivDialogService>();

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
            var token = serviceProvider.GetService<IOptions<AppSettings>>().Value.BotToken;

            var telegramBotClient = new TelegramBotClient(token);
            return telegramBotClient;
        }

        private void SetupLogger()
        {
            var target = new FileTarget
            {
                Layout = "${date:format=HH\\:mm\\:ss.fff}|${logger}|${uppercase:${level}}|${message} ${exception}",
                FileName = "logs" + Path.DirectorySeparatorChar + "${shortdate}" + Path.DirectorySeparatorChar + "nlog-${date:format=yyyy.MM.dd}.log"
            };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, NLog.LogLevel.Trace);
        }
    }
}