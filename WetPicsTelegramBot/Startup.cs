using System;
using System.IO;
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

namespace WetPicsTelegramBot
{
    public class Startup
    {
        public Startup()
        {
            var environmentVariable = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Pattern";
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"AppSettings.{environmentVariable}.json", true, true)
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

            serviceCollection.AddTransient<IDbRepository, DbRepository>();
            serviceCollection.AddTransient<DialogServive>();
            serviceCollection.AddTransient<PhotoPublisherService>();

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

            return new TelegramBotClient(token);
        }

        private void SetupLogger()
        {
            var target = new FileTarget
            {
                Layout = "${date:format=HH\\:mm\\:ss.fff}|${logger}|${uppercase:${level}}|${message} ${exception}",
                FileName = "logs\\${shortdate}\\nlog-${date:format=yyyy.MM.dd}.log"
            };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, NLog.LogLevel.Trace);
        }
    }
}