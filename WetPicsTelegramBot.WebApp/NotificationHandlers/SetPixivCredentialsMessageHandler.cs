using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PixivApi;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers
{
    public class SetPixivCredentialsMessageHandler : MessageHandler
    {
        private readonly IMemoryCache _memoryCache;

        public SetPixivCredentialsMessageHandler(
            ITgClient tgClient,
            ICommandsProvider commandsProvider,
            ILogger<SetPixivCredentialsMessageHandler> logger,
            IMessagesProvider messagesProvider,
            IMemoryCache memoryCache)
            : base(tgClient, logger, commandsProvider, messagesProvider)
        {
            _memoryCache = memoryCache;
        }

        protected override bool WantHandle(Message message, string command) => command.StartsWith("/pixiv");

        protected override Task Handle(Message message, string command, CancellationToken cancellationToken)
        {
            var user = message.From;
            if (user.Id != 66313453)
                return Task.CompletedTask;

            var args = message.Text.Split(' ');
            if (args.Length != 4)
                return Task.CompletedTask;

            var accessToken = args[1];
            var refreshToken = args[2];

            _memoryCache.Set(PixivApiCredentialsCacheKeys.AccessToken, accessToken);
            _memoryCache.Set(PixivApiCredentialsCacheKeys.RefreshToken, refreshToken);

            return Task.CompletedTask;
        }
    }
}
