using System;
using System.Net.Http;
using IqdbApi;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Quartz.Spi;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.WebApp.Factories;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Jobs;
using WetPicsTelegramBot.WebApp.Providers;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services;
using WetPicsTelegramBot.WebApp.Services.Abstract;
using WetPicsTelegramBot.WebApp.Services.PostingServices;

namespace WetPicsTelegramBot.WebApp.StartupConfig
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment CurrentEnvironment { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder.AddNLog());

            services.AddSettings<AppSettings>(Configuration);

            // services 

            services.AddMvc();
            services.AddMediatR();

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
            services.AddTransient<ITelegramImagePreparing, TelegramImagePreparing>();
            services.AddSingleton<HttpClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
                              IHostingEnvironment env,
                              IApplicationLifetime lifetime,
                              IServiceScopeFactory serviceScopeFactory,
                              IServiceProvider container,
                              ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseNlog(env.EnvironmentName)
               .UseWetPicsDbContext()
               .UseDefaultFiles()
               .UseStaticFiles()
               .UseMvc()
               .UseQuartz()
               .UseTelegramBotWebhook();
        }
    }
}