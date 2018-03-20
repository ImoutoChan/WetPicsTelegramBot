using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources
{
    public class AddImageSourceCommandHandler : MessageHandler
    {
        private readonly IAwaitedRepliesService _awaitedRepliesService;

        public AddImageSourceCommandHandler(ITgClient tgClient, 
                                            ILogger<AddImageSourceCommandHandler> logger, 
                                            ICommandsProvider commandsProvider, 
                                            IMessagesProvider messagesProvider,
                                            IAwaitedRepliesService awaitedRepliesService)
            : base(tgClient, logger, commandsProvider, messagesProvider)
        {
            _awaitedRepliesService = awaitedRepliesService;
        }

        protected override bool WantHandle(Message message, string command) 
            => command == CommandsProvider.AddImageSourceCommandText;

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            
            var mes = 
                await TgClient.Reply(message, 
                                     MessagesProvider.SelectImageSource,
                                     cancellationToken,
                                     replyMarkup: GetImageSourceModesKeyboard());

            _awaitedRepliesService.AwaitedReplies
               .TryAdd(mes.MessageId, new SelectImageSourceAwaitedReply());
        }

        private ReplyKeyboardMarkup GetImageSourceModesKeyboard()
        {
            var buttons = Enum
               .GetNames(typeof(ImageSource))
               .Select(x => new KeyboardButton(x))
               .ToList();

            return new ReplyKeyboardMarkup(
                new[] {
                    buttons.Take(6).ToArray(),
                    buttons.Skip(6).ToArray(),
                },
                resizeKeyboard: true,
                oneTimeKeyboard: true);
        }
    }
}