using System.Net.Http;
using IqdbApi;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PixivApi;
using PixivApi.Models;
using Quartz.Spi;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.WebApp.Factories;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Jobs;
using WetPicsTelegramBot.WebApp.Models.Settings;
using WetPicsTelegramBot.WebApp.Providers;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Repositories;
using WetPicsTelegramBot.WebApp.Services;
using WetPicsTelegramBot.WebApp.Services.Abstract;
using WetPicsTelegramBot.WebApp.Services.PostingServices;

namespace WetPicsTelegramBot.WebApp.StartupConfig
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            CurrentEnvironment = environment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment CurrentEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSettings<AppSettings>(Configuration);
            services.Configure<PixivConfiguration>(Configuration.GetSection("Configuration:PixivConfiguration"));
            services.AddHttpClient<IPixivApiClient, PixivApiClient>();
            services.AddTransient<IPixivAuthorization, PixivAuthorization>();

            // services

            services.AddMemoryCache();
            services.AddControllers().AddNewtonsoftJson();
            services.AddMediatR(typeof(Startup));

            services.AddTransient<ICommandsProvider, CommandsProvider>();
            services.AddTransient<IMessagesProvider, MessagesProvider>();
            services.AddTransient<INotificationService, NotificationService>();

            services.AddTelegramClient();

            services.AddTransient<IIqdbService, IqdbService>();
            services.AddTransient<IIqdbClient, IqdbClient>();

            services.AddBooruLoaders();

            services.AddTransient<IRepostSettingsService, RepostSettingsService>();

            services.AddTransient<IDbRepository, DbRepository>();
            services.AddTransient<IImageSourceRepository, ImageSourceRepository>();

            services.AddTransient<IPixivRepository, PixivRepository>();

            services.AddSingleton<IAwaitedRepliesService, AwaitedRepliesService>();

            services.AddTransient<IRepostService, RepostService>();


            services.AddTransient<IJobFactory, InjectableJobFactory>();
            services.AddTransient<PostNextImageSourceJob>();
            services.AddTransient<PostMonthTopJob>();
            services.AddTransient<PostDayTopJob>();
            services.AddTransient<PostWeekTopJob>();

            services.AddTransient<ITopImageDrawService, TopImageDrawService>();
            services.AddTransient<ITopRatingService, TopRatingService>();

            services.AddTransient<IWetpicsService, WetpicsService>();

            services.AddTransient<IScheduledResultsService, ScheduledResultsService>();

            services.AddDatabase();

            services.AddTransient<IPostingServicesFactory, PostingServicesFactory>();
            services.AddTransient<PixivPostingService>();
            services.AddTransient<YanderePostingService>();
            services.AddTransient<DanbooruPostingService>();
            services.AddTransient<IImageSourcePostingService, ImageSourcePostingService>();
            services.AddTransient<ITelegramImagePreparing, NetVipsTelegramImagePreparer>();
            services.AddSingleton<HttpClient>();

            services.AddTransient<IPolicesFactory, PolicesFactory>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseWetPicsDbContext()
                .UseQuartz()
                .UseTelegramBotWebhook();

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseEndpoints(x => x.MapControllers());
        }
    }
}
