using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.Stats
{
    public class MyStatsMessageHandler : MessageHandler
    {
        private readonly IDbRepository _dbRepository;

        public MyStatsMessageHandler(ITgClient tgClient,
                                     ICommandsProvider commandsProvider,
                                     ILogger<MyStatsMessageHandler> logger,
                                     IMessagesProvider messagesProvider,
                                     IDbRepository dbRepository)
            : base(tgClient, 
                   logger,
                   commandsProvider,
                   messagesProvider)
        {
            _dbRepository = dbRepository;
        }

        protected override bool WantHandle(Message message, string command) 
            => command == CommandsProvider.MyStatsCommandText;

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            var user = message.From;

            var result = await _dbRepository.GetStats(user.Id, message.Chat.Id);

            var reply = String.Format(MessagesProvider.StatsResultF.Message,
                                      user.GetBeautyName(),
                                      result.PicCount,
                                      result.GetLikeCount,
                                      result.SetLikeCount,
                                      result.SetSelfLikeCount);

            await TgClient.Reply(message, 
                                 reply, 
                                 cancellationToken, 
                                 parseMode: MessagesProvider.StatsResultF.ParseMode);
        }
    }
}