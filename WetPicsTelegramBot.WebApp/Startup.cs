using Imouto.BooruParser.Loaders;
using IqdbApi;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using Quartz.Spi;
using System;
using System.Linq;
using System.Net.Http;
using Telegram.Bot;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.Data.Context;
using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.WebApp.Factories;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Jobs;
using WetPicsTelegramBot.WebApp.Providers;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp
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
            services.AddLogging();

            // configuration

            services.AddOptions();
            services.Configure<AppSettings>(Configuration.GetSection("Configuration"));
            services.AddTransient<AppSettings>(ser => ser.GetService<IOptions<AppSettings>>().Value);

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
            services.AddTransient<DanbooruLoader>(CreateDanbooruLoader);
            services.AddTransient<SankakuLoader>(CreateSankakuLoader);
            services.AddTransient<YandereLoader>();

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

            UseNLog(loggerFactory);

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseMvc();

            EnsureWetPicsDbContextMigration(app.ApplicationServices);
            

            var quartz = new QuartzStartup(serviceScopeFactory);
            lifetime.ApplicationStarted.Register(quartz.Start);
            lifetime.ApplicationStopping.Register(quartz.Stop);

            lifetime.ApplicationStarted.Register(() =>
            {
                var logger = container.GetService<ILogger<Startup>>();


                var adress = container.GetService<AppSettings>().WebHookAdress;

                logger.LogInformation($"Setting webhook to {adress}");
                app.ApplicationServices.GetService<ITelegramBotClient>().SetWebhookAsync(adress, maxConnections: 5).Wait();
                logger.LogInformation($"Webhook is set to {adress}");

                logger.LogInformation($"Webhook info: {JsonConvert.SerializeObject(app.ApplicationServices.GetService<ITelegramBotClient>().GetWebhookInfoAsync().Result)}");
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                var logger = container.GetService<ILogger<Startup>>();

                app.ApplicationServices.GetService<ITelegramBotClient>().DeleteWebhookAsync().Wait();
                logger.LogInformation("Webhook removed");
            });
        }

        private void UseNLog(ILoggerFactory loggerFactory)
        {
            var configFileRelativePath = $"config/nlog.{CurrentEnvironment.EnvironmentName}.config";

            loggerFactory.AddNLog();
            loggerFactory.ConfigureNLog(configFileRelativePath);
        }
        
        private static void EnsureWetPicsDbContextMigration(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<Program>>();

            try
            {
                using (var serviceScope = serviceProvider.CreateScope())
                using (var db = serviceScope.ServiceProvider.GetService<WetPicsDbContext>())
                {
                    db.Database.Migrate();

                    var botId = Int32.Parse(serviceProvider.GetService<AppSettings>().BotToken.Split(':').First());

                    var botUser = db.ChatUsers.FirstOrDefault(x => x.UserId == botId);

                    if (botUser == null)
                    {
                        var chatuser = new ChatUser { FirstName = "WetPicsBot", UserId = botId };

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

        private SankakuLoader CreateSankakuLoader(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<AppSettings>().SankakuConfiguration;

            return new SankakuLoader(config.Login, config.PassHash, config.Delay);
        }

        private DanbooruLoader CreateDanbooruLoader(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<AppSettings>().DanbooruConfiguration;

            return new DanbooruLoader(config.Login, config.ApiKey, config.Delay);
        }
        
        private ITelegramBotClient CreateTelegramBotClient(IServiceProvider serviceProvider)
        {
            var token = serviceProvider.GetService<AppSettings>().BotToken;

            var telegramBotClient = new TelegramBotClient(token);
            return telegramBotClient;
        }
    }
}