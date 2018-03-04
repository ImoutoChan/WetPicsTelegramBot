using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers
{
    public class ForwardRequestCallbackHandler : CallbackHandler
    {
        public ForwardRequestCallbackHandler(ITgClient tgClient,
                                             ICommandsProvider commandsProvider,
                                             ILogger<ForwardRequestCallbackHandler> logger,
                                             IMessagesProvider messagesProvider)
            : base(tgClient, 
                   logger,
                   commandsProvider,
                   messagesProvider)
        {
        }

        protected override bool WantHandle(CallbackQuery query)
            => IsRepost(query);

        private bool IsRepost(CallbackQuery query) 
            => query.Data.StartsWith("forward_request");

        protected override async Task Handle(CallbackQuery query, CancellationToken cancellationToken)
        {
            var forwardsParts = query.Data.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            
            var forward = forwardsParts.Last();
            var repostParts = forward.Split(new[] { '_' });

            var chatIdString = repostParts.First().Split(new[] { '#' }).Last();
            var messageIdString = repostParts.Last().Split(new[] { '#' }).Last();

            var chatId = Int64.Parse(chatIdString);
            var messageId = Int32.Parse(messageIdString);

            await TgClient.Client.ForwardMessageAsync(query.Message.Chat.Id, 
                                                      chatId, 
                                                      messageId, 
                                                      cancellationToken: cancellationToken);

            await TgClient.Client.AnswerCallbackQueryAsync(query.Id, 
                                                           cancellationToken: cancellationToken);
        }
    }
}