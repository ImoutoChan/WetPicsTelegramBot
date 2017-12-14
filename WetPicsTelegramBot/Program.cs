using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WetPicsTelegramBot.Database.Context;
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

                    var botId = Int32.Parse(serviceProvider.GetService<AppSettings>().BotToken.Split(':').First());

                    var botUser = db.ChatUsers.FirstOrDefault(x => x.UserId == botId);

                    if (botUser == null)
                    {
                        var chatuser = new ChatUser {FirstName = "WetPicsBot", UserId = botId};

                        db.ChatUsers.Add(chatuser);
                        db.SaveChanges();
                    }
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