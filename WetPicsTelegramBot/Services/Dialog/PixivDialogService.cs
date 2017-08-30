using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Helpers;
using WetPicsTelegramBot.Models;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services.Dialog
{
    class PixivDialogService : IDialogService<PixivDialogService>
    {
        private readonly IDialogObserverService _baseDialogService;
        private readonly ILogger<PixivDialogService> _logger;
        private readonly IMessagesService _messagesService;
        private readonly ICommandsService _commandsService;
        private readonly IPixivSettingsService _pixivSettings;
        private readonly ITelegramBotClient _telegramApi;

        private Dictionary<string, Func<Command, Task>> _commandHandlers;
        private Dictionary<IDictionary, Func<Message, Task>> _replyHandlers;
        
        // MessageId, PixivMode
        private readonly Dictionary<int, PixivTopType> _awaitIntervalReply = new Dictionary<int, PixivTopType>();

        // MessageId
        private readonly Dictionary<int, object> _awaitModeReply = new Dictionary<int, object>();

        public PixivDialogService(IDialogObserverService baseDialogService,
                                    ILogger<PixivDialogService> logger,
                                    IMessagesService messagesService,
                                    ICommandsService commandsService,
                                    IPixivSettingsService pixivSettings,
                                    ITelegramBotClient telegramApi)
        {
            _baseDialogService = baseDialogService;
            _logger = logger;
            _messagesService = messagesService;
            _commandsService = commandsService;
            _pixivSettings = pixivSettings;
            _telegramApi = telegramApi;

            SetupCommandHandlers();
            SetupReplyHandlers();
        }

        private void SetupCommandHandlers()
        {
            _commandHandlers = new Dictionary<string, Func<Command, Task>>
            {
                {_commandsService.ActivatePixivCommandText, OnNextActivatePixivCommand},
                {_commandsService.DeactivatePixivCommandText, OnNextDeactivatePixivCommand}
            };
        }

        private void SetupReplyHandlers()
        {
            _replyHandlers = new Dictionary<IDictionary, Func<Message, Task>>
            {
                {_awaitModeReply, OnNextSelectPixivModeReply},
                {_awaitIntervalReply, OnNextSelectPixivIntervalReply}
            };
        }

        public void Subscribe()
        {
            _baseDialogService
                .MessageObservable
                .GroupBy(x => x.CommandName)
                .Where(group => _commandHandlers.ContainsKey(group.Key))
                .Subscribe(group => group.HandleAsync(_commandHandlers[group.Key]).Subscribe());

            _baseDialogService
                .RepliesObservable
                .GroupBy(x => _replyHandlers.Keys.FirstOrDefault(y => y.Contains(x.ReplyToMessage.MessageId)))
                .Where(group => group.Key != null)
                .Subscribe(group => group.HandleAsync(_replyHandlers[group.Key]).Subscribe());
        }
        
        private async Task OnNextDeactivatePixivCommand(Command command)
        {
            _logger.LogTrace($"{command.CommandName} command recieved");
            var message = command.Message;
            
            await _pixivSettings.Remove(message.Chat.Id);

            await _telegramApi.SendTextMessageAsync(message.Chat.Id, _messagesService.PixivWasDeactivated);
        }

        private async Task OnNextActivatePixivCommand(Command command)
        {
            _logger.LogTrace($"{command.CommandName} command recieved");
            var message = command.Message;
            
            var mes = await _baseDialogService.Reply(message, _messagesService.SelectPixivModeMessage, replyMarkup: GetPhotoKeyboard());

            _awaitModeReply.Add(mes.MessageId, null);
        }
        
        private async Task OnNextSelectPixivIntervalReply(Message message)
        {
            _logger.LogTrace($"\"{message.Text}\" reply recieved");

            var replyTo = message.ReplyToMessage.MessageId;

            if (!_awaitIntervalReply.ContainsKey(replyTo))
            {
                return;
            }
            
            var mode = _awaitIntervalReply[replyTo];

            if (!Int32.TryParse(message.Text, out var interval))
            {
                await _baseDialogService.Reply(message, _messagesService.PixivIncorrectInterval);
                return;
            }

            await _pixivSettings.Add(message.Chat.Id, mode, interval);
            
            await _baseDialogService.Reply(message, _messagesService.PixivWasActivated);

            _awaitIntervalReply.Remove(replyTo);
        }

        private async Task OnNextSelectPixivModeReply(Message message)
        {
            _logger.LogTrace($"\"{message.Text}\" reply recieved");

            var mode = message.Text;
            if (!Enum.TryParse(mode, out PixivTopType myStatus) || !myStatus.IsDefined())
            {
                await _baseDialogService.Reply(message, _messagesService.PixivIncorrectMode);
                return;
            }

            var mes = await _baseDialogService.Reply(message,
                                String.Format(_messagesService.SelectPixivIntervalMessageF, message.Text),
                                replyMarkup: new ForceReply { Force = true, Selective = true });

            _awaitIntervalReply.Add(mes.MessageId, myStatus);
            _awaitModeReply.Remove(message.ReplyToMessage.MessageId);
        }

        private ReplyKeyboardMarkup GetPhotoKeyboard()
        {
            return new ReplyKeyboardMarkup(
                new[] {
                    new[]
                    {
                        new KeyboardButton("DailyGeneral"),
                        new KeyboardButton("DailyR18"),
                        new KeyboardButton("WeeklyGeneral"),
                        new KeyboardButton("WeeklyR18"),
                        new KeyboardButton("Monthly"),
                        new KeyboardButton("Rookie"),
                    },
                    new[]
                    {
                        new KeyboardButton("Original"),
                        new KeyboardButton("ByMaleGeneral"),
                        new KeyboardButton("ByMaleR18"),
                        new KeyboardButton("ByFemaleGeneral"),
                        new KeyboardButton("ByFemaleR18"),
                        new KeyboardButton("R18G"),
                    },
                },
                resizeKeyboard: true,
                oneTimeKeyboard: true
            );
        }
    }
}