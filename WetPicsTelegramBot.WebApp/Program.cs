using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using WetPicsTelegramBot.WebApp.StartupConfig;

namespace WetPicsTelegramBot.WebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureApp)
                .ConfigureLogging(ConfigureLogging)
                .ConfigureWebHostDefaults(
                    webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                    })
                .UseNLog();

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Trace);

            var environment = GetEnvironment(context);

            var configFileRelativePath = $"config/nlog.{environment}.config";

            NLogBuilder.ConfigureNLog(configFileRelativePath);
        }

        private static void ConfigureApp(HostBuilderContext builderContext, IConfigurationBuilder config)
        {
            var environment = GetEnvironment(builderContext);

            config
                .AddJsonFile($"config/AppSettings.{environment}.json", true, true)
                .AddJsonFile("appsettings.Cache.json.backup", true)
                .AddJsonFile("appsettings.Cache.json", true)
                .AddEnvironmentVariables();
        }

        private static string GetEnvironment(HostBuilderContext builderContext)
        {
            return

            // fix for external debugers
#if DEBUG
            "Development";
#else
            builderContext.HostingEnvironment.EnvironmentName;
#endif
        }
    }
}
