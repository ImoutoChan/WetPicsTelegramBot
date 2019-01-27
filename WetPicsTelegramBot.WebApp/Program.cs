using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;
using WetPicsTelegramBot.WebApp.StartupConfig;

namespace WetPicsTelegramBot.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
            => WebHost.CreateDefaultBuilder(args)
                      .ConfigureAppConfiguration(ConfigureApp)
                      .ConfigureLogging(ConfigureLogging)
                      .UseStartup<Startup>()
                      .UseNLog()
                      .Build();

        private static void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder builder)
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Trace);

            var environment = GetEnvironment(context);

            var configFileRelativePath = $"config/nlog.{environment}.config";

            NLogBuilder.ConfigureNLog(configFileRelativePath);
        }

        private static void ConfigureApp(WebHostBuilderContext builderContext, IConfigurationBuilder config)
        {
            var environment = GetEnvironment(builderContext);

            config.AddJsonFile($"config/AppSettings.{environment}.json", true, true).AddEnvironmentVariables();
        }

        private static string GetEnvironment(WebHostBuilderContext builderContext)
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
