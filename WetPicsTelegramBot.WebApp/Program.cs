using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace WetPicsTelegramBot.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    var environment =

// fix for external debugers
#if DEBUG
                        "Development";
#else
                        builderContext.HostingEnvironment.EnvironmentName;
#endif

                    config.AddJsonFile($"config/AppSettings.{environment}.json", true, true)
                          .AddEnvironmentVariables();
                })
                .UseStartup<Startup>()
                .Build();
    }
}
