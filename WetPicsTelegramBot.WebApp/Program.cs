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
                    config.AddJsonFile($"config/AppSettings.{builderContext.HostingEnvironment.EnvironmentName}.json", true, true)
                          .AddEnvironmentVariables();
                })
                .UseStartup<Startup>()
                .Build();
    }
}
