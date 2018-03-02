using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using Telegram.Bot;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // logger
            var configFileRelativePath = $"config/nlog.{CurrentEnvironment.EnvironmentName}.config";

            var nlog = new LoggerFactory().AddNLog();
            nlog.ConfigureNLog(configFileRelativePath);

            services.AddSingleton(nlog);
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
            services.AddSingleton<ITelegramBotClient>(CreateTelegramBotClient);

            services.AddTransient<IIqdbService, IqdbService>();

            services.AddMediatR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            var adress = app.ApplicationServices.GetService<AppSettings>().WebHookAdress;
            app.ApplicationServices.GetService<ITelegramBotClient>().SetWebhookAsync(adress).Wait();
        }

        private ITelegramBotClient CreateTelegramBotClient(IServiceProvider serviceProvider)
        {
            var token = serviceProvider.GetService<AppSettings>().BotToken;

            var telegramBotClient = new TelegramBotClient(token);
            return telegramBotClient;
        }
    }
}
