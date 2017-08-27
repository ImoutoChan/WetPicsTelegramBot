using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace WetPicsTelegramBot.Services.Dialog
{
    internal class HelpDialogService
    {
        private readonly IBaseDialogService _baseDialogService;
        private readonly ILogger<HelpDialogService> _logger;
        private readonly IMessagesService _messagesService;

        public HelpDialogService(IBaseDialogService baseDialogService,
                                 ILogger<HelpDialogService> logger,
                                 IMessagesService messagesService)
        {
            _baseDialogService = baseDialogService;
            _logger = logger;
            _messagesService = messagesService;

            _baseDialogService.CommandReceived += BaseDialogServiceOnCommandReceived;
        }

        private async void BaseDialogServiceOnCommandReceived(object sender, CommandEventArgs commandEventArgs)
        {
            if (commandEventArgs.Command != _messagesService.HelpCommandText 
                && commandEventArgs.Command != _messagesService.StartCommandText)
                return;

            _logger.LogTrace($"{commandEventArgs.Command} command recieved");

            var text = _messagesService.HelpMessage;

            await _baseDialogService.Reply(commandEventArgs.Message, text, ParseMode.Markdown);
        }
    }

    internal class BaseDialogService : IBaseDialogService
    {
        private readonly ITelegramBotClient _api;
        private readonly ILogger<BaseDialogService> _logger;

        private User _me;

        public BaseDialogService(ITelegramBotClient api,
                                 ILogger<BaseDialogService> logger)
        {
            _api = api;
            _logger = logger;

            _api.OnMessage += BotOnMessageReceived;
        }
        private async Task<User> GetMe()
        {
            return _me ?? (_me = await _api.GetMeAsync());
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message?.Type != MessageType.TextMessage)
                return;

            var me = await GetMe();
            var username = me.Username;

            var fullCommand = message.Text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First();
            var isCommandWithId = fullCommand.EndsWith(username);
            var command = isCommandWithId ? fullCommand.Split('@').First() : fullCommand;

            OnCommandReceived(command, message);
        }

        public event EventHandler<CommandEventArgs> CommandReceived;

        private void OnCommandReceived(string command, Message message)
        {
            CommandReceived?.Invoke(this, new CommandEventArgs(command, message));
        }

        public async Task Reply(Message message,
                                string text,
                                ParseMode parseMode = ParseMode.Default,
                                IReplyMarkup replyMarkup = null)
        {
            await _api.SendTextMessageAsync(message.Chat.Id, 
                                            text, 
                                            parseMode, 
                                            replyToMessageId: message.MessageId, 
                                            replyMarkup: replyMarkup);
        }
    }
}
