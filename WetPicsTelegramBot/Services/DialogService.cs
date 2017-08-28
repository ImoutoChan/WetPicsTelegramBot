using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Database.Model;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services
{
    internal class DialogService
    {
        private readonly ILogger<DialogService> _logger;
        private readonly IChatSettings _chatSettings;
        private readonly IDbRepository _dbRepository;
        private readonly IPixivSettings _pixivSettings;
        private readonly IBaseDialogService _baseDialogService;
        private readonly IMessagesService _messagesService;
        private readonly ICommandsService _commandsService;
        private readonly ITelegramBotClient _api;

        private User _me;
        

        public DialogService(ITelegramBotClient api, 
                                ILogger<DialogService> logger, 
                                IChatSettings chatSettings,
                                IDbRepository dbRepository, 
                                IPixivSettings pixivSettings,
                                IBaseDialogService baseDialogService,
                                IMessagesService messagesService,
                                ICommandsService commandsService)
        {
            _api = api;
            _logger = logger;
            _chatSettings = chatSettings;
            _dbRepository = dbRepository;
            _pixivSettings = pixivSettings;
            _baseDialogService = baseDialogService;
            _messagesService = messagesService;
            _commandsService = commandsService;

            _api.OnMessage += BotOnMessageReceived;
        }

        private async Task<User> GetMe()
        {
            return _me ?? (_me = await _api.GetMeAsync());
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;

                if (message?.Type != MessageType.TextMessage)
                    return;

                var me = await GetMe();
                var username = me.Username;

                var fullCommand = message.Text.Split(new [] {" "}, StringSplitOptions.RemoveEmptyEntries).First();
                var isCommandWithId = fullCommand.EndsWith(username);
                var command = (isCommandWithId) ? fullCommand.Split('@').First() : fullCommand;


                // if reply to me
                else if (message.ReplyToMessage?.From != null 
                            && message.ReplyToMessage.From.Id == me.Id)
                {
                    if ((bool) message.ReplyToMessage?.Text.StartsWith(_messagesService.SelectPixivModeMessage))
                    {
                        await ActivatePixivSelectedModeCommand(message);
                    }
                    else if ((bool)message.ReplyToMessage?.Text.EndsWith(_messagesService.SelectPixivIntervalMessage))
                    {
                        await ActivatePixivSelectedIntervalCommand(message);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"unable to process message" + e.ToString());
            }
        }

        private async Task ActivatePixivSelectedIntervalCommand(Message message)
        {
            var mode = message.ReplyToMessage.Text.Split('\n')
                .First()
                .Split(' ')
                .Last();

            if (!Enum.TryParse(mode, out PixivTopType myStatus))
            {
                await _api.SendTextMessageAsync(message.Chat.Id, "Выбран некорректный режим", replyToMessageId: message.MessageId);
                return;
            }

            if (!Int32.TryParse(message.Text, out var interval))
            {
                await _api.SendTextMessageAsync(message.Chat.Id, "Введен некорректный интервал", replyToMessageId: message.MessageId);
                return;
            }

            await _pixivSettings.Add(message.Chat.Id, myStatus, interval);

            await _api.SendTextMessageAsync(message.Chat.Id, "Пиксив активирован!", replyToMessageId: message.MessageId);
        }

        private async Task ActivatePixivSelectedModeCommand(Message message)
        {
            await _api.SendTextMessageAsync(message.Chat.Id,
                    "Выбран режим: " + message.Text + Environment.NewLine + _messagesService.SelectPixivIntervalMessage,
                    replyToMessageId: message.MessageId, 
                    replyMarkup: new ForceReply { Force = true, Selective = true } );
        }
        
        private void LogCommand(string startCommandText)
        {
            _logger.LogTrace($"{startCommandText} command recieved");
        }
    }
}