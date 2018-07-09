using System;
using System.Net.Http;
using Imouto.BooruParser.Loaders;
using IqdbApi;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Quartz.Spi;
using SixLabors.ImageSharp.Memory;
using Telegram.Bot;
using Telegram.Bot.QueuedWrapper;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.Data.Context;
using WetPicsTelegramBot.WebApp.Factories;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Jobs;
using WetPicsTelegramBot.WebApp.Providers;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services;
using WetPicsTelegramBot.WebApp.Services.Abstract;
using WetPicsTelegramBot.WebApp.Services.PostingServices;

namespace WetPicsTelegramBot.WebApp.Startup
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
            services.AddTransient<IMediator, Mediator>();
            services.AddTransient<ICommandsProvider, CommandsProvider>();
            services.AddTransient<IMessagesProvider, MessagesProvider>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<ITgClient, TgClient>();
            services.AddTransient<ITelegramBotClient>(CreateTelegramBotClient);

            services.AddTransient<IIqdbService, IqdbService>();

            services.AddTransient<IIqdbClient, IqdbClient>();
            services.AddBooruLoaders();

            services.AddTransient<IRepostSettingsService, RepostSettingsService>();

            services.AddTransient<IDbRepository, DbRepository>();
            services.AddTransient<IImageSourceRepository, ImageSourceRepository>();

            services.AddSingleton<IAwaitedRepliesService, AwaitedRepliesService>();

            services.AddTransient<IRepostService, RepostService>();

            services.AddMediatR();

            services.AddTransient<IJobFactory, InjectableJobFactory>();
            services.AddTransient<PostNextImageSourceJob>();
            services.AddTransient<PostMonthTopJob>();
            services.AddTransient<PostDayTopJob>();

            services.AddTransient<ITopImageDrawService, TopImageDrawService>();
            services.AddTransient<ITopRatingService, TopRatingService>();

            services.AddTransient<IWetpicsService, WetpicsService>();

            services.AddTransient<IScheduledResultsService, ScheduledResultsService>();

            services.AddDbContext<WetPicsDbContext>((serviceProvider, optionBuilder) =>
            {
                var connectionString = serviceProvider.GetService<AppSettings>().ConnectionString;
                optionBuilder.UseNpgsql(connectionString);
            }, ServiceLifetime.Transient);

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
        
        private ITelegramBotClient CreateTelegramBotClient(IServiceProvider serviceProvider)
        {
            var token = serviceProvider.GetService<AppSettings>().BotToken;
            var httpClient = serviceProvider.GetService<HttpClient>();

            var telegramBotClient = new QueuedTelegramBotClient(token, httpClient);
            return telegramBotClient;
        }
    }
}