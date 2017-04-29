using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
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
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            serviceProvider.GetService<App>().Run();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // logger
            serviceCollection.AddSingleton(new LoggerFactory().AddNLog());
            serviceCollection.AddLogging();
            SetupLogger();

            // configuration
            var environmentVariable = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile($"AppSettings.{environmentVariable}.json", true, true)
                                    .AddEnvironmentVariables()
                                    .Build();
            serviceCollection.AddOptions();
            serviceCollection.Configure<AppSettings>(configuration.GetSection("Configuration"));

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

        private static ITelegramBotClient CreateTelegramBotClient(IServiceProvider serviceProvider)
        {
            var token = serviceProvider.GetService<IOptions<AppSettings>>().Value.BotToken;

            return new TelegramBotClient(token);
        }

        private static void SetupLogger()
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