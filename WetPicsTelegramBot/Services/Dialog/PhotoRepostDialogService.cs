using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
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
        private readonly IBaseDialogService _baseDialogService;
        private readonly ILogger<PixivDialogService> _logger;
        private readonly IMessagesService _messagesService;
        private readonly ICommandsService _commandsService;
        private readonly IPixivSettings _pixivSettings;
        private readonly ITelegramBotClient _telegramApi;

        private Dictionary<string, Func<Command, Task>> _commandHandlers;
        private Dictionary<string, Func<Message, Task>> _replyHandlers;
        
        public PixivDialogService(IBaseDialogService baseDialogService,
                                    ILogger<PixivDialogService> logger,
                                    IMessagesService messagesService,
                                    ICommandsService commandsService,
                                    IPixivSettings pixivSettings,
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
            _replyHandlers = new Dictionary<string, Func<Message, Task>>
            {
                {_messagesService.SelectPixivModeMessage.RemoveWhiteSpaces(), OnNextSelectPixivModeReply},
                {_messagesService.SelectPixivIntervalMessage.RemoveWhiteSpaces(), OnNextSelectPixivIntervalReply}
            };
        }

        public void Subscribe()
        {
            _baseDialogService
                .MessageObservable
                .GroupBy(x => x.CommandName)
                .Where(group => _commandHandlers.ContainsKey(group.Key))
                .Subscribe(group => group.Subscribe(command => _commandHandlers[group.Key](command).Wait()));

            _baseDialogService
                .RepliesObservable
                .GroupBy(x => x.ReplyToMessage.Text.RemoveWhiteSpaces())
                .Where(group => _commandHandlers.ContainsKey(group.Key))
                .Subscribe(group => group.Subscribe(message => _replyHandlers[group.Key](message).Wait()));
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
            
            await _baseDialogService.Reply(message, _messagesService.SelectPixivModeMessage, replyMarkup: GetPhotoKeyboard());
        }
        
        private ReplyKeyboardMarkup GetPhotoKeyboard()
        {
            return new ReplyKeyboardMarkup(
                new [] {
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

        private Task OnNextSelectPixivIntervalReply(Message arg)
        {
            throw new NotImplementedException();
        }

        private async Task OnNextSelectPixivModeReply(Message message)
        {
            await _baseDialogService.Reply(message, $"Выбран режим: {message.Text}{Environment.NewLine}{_messagesService.SelectPixivIntervalMessage}", 
                replyMarkup: new ForceReply { Force = true, Selective = true } );
        }
    }
    
    internal class RepostDialogService : IDialogService<RepostDialogService>
    {
        private readonly IBaseDialogService _baseDialogService;
        private readonly ILogger<RepostDialogService> _logger;
        private readonly IMessagesService _messagesService;
        private readonly ICommandsService _commandsService;
        private readonly IChatSettings _chatSettings;
        private readonly ITelegramBotClient _telegramApi;

        private Dictionary<string, Func<Command, Task>> _commandHandlers;

        public RepostDialogService(IBaseDialogService baseDialogService,
                                            ILogger<RepostDialogService> logger,
                                            IMessagesService messagesService,
                                            ICommandsService commandsService,
                                            IChatSettings chatSettings,
                                            ITelegramBotClient telegramApi)
        {
            _baseDialogService = baseDialogService;
            _logger = logger;
            _messagesService = messagesService;
            _commandsService = commandsService;
            _chatSettings = chatSettings;
            _telegramApi = telegramApi;

            SetupCommandHandlers();
        }

        private void SetupCommandHandlers()
        {
            _commandHandlers = new Dictionary<string, Func<Command, Task>>
            {
                {_commandsService.ActivatePhotoRepostCommandText, OnNextActivateRepostCommand},
                {_commandsService.ActivatePhotoRepostHelpCommandText, OnNextRepostHelpCommand},
                {_commandsService.DeactivatePhotoRepostCommandText, OnNextDeactivateRepostCommand},
            };
        }

        public void Subscribe()
        {
            _baseDialogService
                .MessageObservable
                .GroupBy(x => x.CommandName)
                .Where(group => _commandHandlers.ContainsKey(group.Key))
                .Subscribe(group => group.Subscribe(command => _commandHandlers[group.Key](command).Wait()));

            _baseDialogService
                .RepliesObservable
                .Where(IsReplyToActivateRepost)
                .Subscribe(message => OnNextActivateReply(message).Wait());
        }

        private bool IsReplyToActivateRepost(Message x) 
            => x.ReplyToMessage.Text.RemoveWhiteSpaces() == _messagesService.ActivateRepostMessage.RemoveWhiteSpaces();

        private async Task OnNextActivateReply(Message message)
        {
            _logger.LogTrace($"\"{message.Text}\" reply recieved");

            var targetId = BuildId(message.Text);

            if (targetId == null)
            {
                await _baseDialogService.Reply(message, _messagesService.RepostWrongIdFormat);
            }
            else
            {
                await SetRepostId(targetId, message);
            }
        }

        private string BuildId(string inputId)
        {
            if (String.IsNullOrWhiteSpace(inputId))
            {
                return null;
            }

            var firstLetter = inputId[0];
            string idString = null;
            switch (firstLetter)
            {
                case '@':
                    idString = inputId;
                    break;
                case 'u':
                case 'g':
                    idString = "-" + inputId.Substring(1);
                    break;
                case 'c':
                    idString = "-100" + inputId.Substring(1);
                    break;
                default:
                    return null;
            }
            

            return idString;
        }

        private async Task SetRepostId(string targetChatId, Message message)
        {
            try
            {
                var mes = await _telegramApi.SendTextMessageAsync(targetChatId, _messagesService.RepostActivateTargetSuccess);

                await _chatSettings.Add(message.Chat.Id, targetChatId);

                await _telegramApi.SendTextMessageAsync(message.Chat.Id, _messagesService.RepostActivateSourceSuccess);
            }
            catch (Exception e)
            {
                try
                {
                    await _baseDialogService.Reply(message, _messagesService.RepostActivateSourceFailure);
                    // TODO add exception to log
                    _logger.LogError($"Unable to set repost id");
                }
                catch (Exception e1)
                {
                    // TODO add exception to log
                    _logger.LogError($"Unable to send repost id error");
                }
            }
        }

        private async Task OnNextRepostHelpCommand(Command command)
        {
            _logger.LogTrace($"{command.CommandName} command recieved");
            var text = _messagesService.RepostHelpMessage;

            await _baseDialogService.Reply(command.Message, text).ConfigureAwait(false);
        }

        private async Task OnNextDeactivateRepostCommand(Command command)
        {
            _logger.LogTrace($"{command.CommandName} command recieved");

            await _chatSettings.Remove(command.Message.Chat.Id).ConfigureAwait(false);

            var text = _messagesService.DeactivatePhotoRepostMessage;

            await _baseDialogService.Reply(command.Message, text).ConfigureAwait(false);
        }

        private async Task OnNextActivateRepostCommand(Command command)
        {
            _logger.LogTrace($"{command.CommandName} command recieved");

            var text = _messagesService.ActivateRepostMessage;

            await _baseDialogService.Reply(command.Message, text, replyMarkup: new ForceReply { Force = true }).ConfigureAwait(false);
        }
    }
}