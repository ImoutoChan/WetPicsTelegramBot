using System;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.Extensions.DependencyInjection;

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

            serviceProvider.GetService<App>().Run();
        }
    }
}