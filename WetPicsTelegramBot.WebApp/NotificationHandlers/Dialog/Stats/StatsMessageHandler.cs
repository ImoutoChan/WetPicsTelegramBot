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
    public class StatsMessageHandler : MessageHandler
    {
        private readonly IDbRepository _dbRepository;

        public StatsMessageHandler(ITgClient tgClient,
                                   ICommandsProvider commandsProvider,
                                   ILogger<StatsMessageHandler> logger,
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
            => command == CommandsProvider.StatsCommandText;

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            if (message.ReplyToMessage == null)
            {
                await TgClient.Reply(message, MessagesProvider.StatsReplyToUser, cancellationToken);
                return;
            }

            var user = message.ReplyToMessage.From;

            var result = await _dbRepository.GetStats(user.Id);

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