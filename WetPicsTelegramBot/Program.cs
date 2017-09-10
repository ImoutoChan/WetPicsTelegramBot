using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WetPicsTelegramBot.Database.Context;
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
            startup.ConfigureServices(serviceCollection, he);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.GetService<ILogger<Program>>().LogDebug($"Environment: {he.EnvironmentName}");

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
                logger.LogError(e, "Exception occurred in migration process");
                throw;
            }
        }
    }
}