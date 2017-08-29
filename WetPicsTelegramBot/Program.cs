using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WetPicsTelegramBot.Database.Model;
using WetPicsTelegramBot.Helpers;

namespace WetPicsTelegramBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            var environmentVariable = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var he = new HostingEnvironment
            {
                EnvironmentName = !String.IsNullOrWhiteSpace(environmentVariable) ? environmentVariable : "Development"
            };

            var startup = new Startup(he);
            startup.ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            EnsureWetPicsDbContextMigration(serviceProvider);

            serviceProvider.GetService<App>().Run();
        }

        private static void EnsureWetPicsDbContextMigration(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<Program>>();

            try
            {
                using (var db = serviceProvider.GetService<WetPicsDbContext>())
                {
                    db.Database.Migrate();
                }
            }
            catch (Exception e)
            {
                logger.LogError("unable to migrate" + e.Message);
                throw;
            }
        }
    }
}