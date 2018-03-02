using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WetPicsTelegramBot.Data.Context
{
    class DesignTimeWetPicsDbContextFactory : IDesignTimeDbContextFactory<WetPicsDbContext>
    {
        public WetPicsDbContext CreateDbContext(string[] args)
        {
            var connectionString = "Server=postgresserver;Port=5432;Database=wetpics;User Id=postgres;Password=postgres;";

            var optionsBuilder = new DbContextOptionsBuilder<WetPicsDbContext>();

            optionsBuilder.UseNpgsql(connectionString);

            return new WetPicsDbContext(optionsBuilder.Options);
        }
    }
}