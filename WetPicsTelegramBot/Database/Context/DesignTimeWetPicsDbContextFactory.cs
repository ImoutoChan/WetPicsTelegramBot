using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using WetPicsTelegramBot.Database.Context;

namespace WetPicsTelegramBot.Database.Model.Context
{
    class DesignTimeWetPicsDbContextFactory : IDesignTimeDbContextFactory<WetPicsDbContext>
    {
        public WetPicsDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile($"AppSettings.Development.json", true, true)
                    .AddEnvironmentVariables()
                    .Build()
                    .GetSection("Configuration")
                    ["ConnectionString"];

            var optionsBuilder = new DbContextOptionsBuilder<WetPicsDbContext>();

            optionsBuilder.UseNpgsql(connectionString);

            return new WetPicsDbContext(optionsBuilder.Options);
        }
    }
}