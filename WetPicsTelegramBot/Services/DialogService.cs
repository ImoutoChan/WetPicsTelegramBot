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

                if (command == _commandsService.MyStatsCommandText)
                {
                    await ProcessMyStatsCommand(message);
                }
                else if (command == _commandsService.StatsCommandText)
                {
                    await ProcessStatsCommand(message);
                }
                else if (command == _commandsService.ActivatePixivCommandText)
                {
                    await ActivatePixivCommand(message);
                }
                else if (command == _commandsService.DeactivatePixivCommandText)
                {
                    await DeactivatePixivCommand(message);
                }

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

        private async Task DeactivatePixivCommand(Message message)
        {
            LogCommand(_commandsService.DeactivatePixivCommandText);
            
            await _pixivSettings.Remove(message.Chat.Id);

            await _api.SendTextMessageAsync(message.Chat.Id, "Пиксив деактивирован!");
        }

        private async Task ActivatePixivCommand(Message message)
        {
            LogCommand(_commandsService.ActivatePixivCommandText);

            await _api.SendTextMessageAsync(message.Chat.Id,
                    _messagesService.SelectPixivModeMessage,
                    replyToMessageId: message.MessageId,
                    replyMarkup: GetPhotoKeyboard());
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

        private async Task ProcessStatsCommand(Message message)
        {
            LogCommand(_commandsService.StatsCommandText);

            if (message.ReplyToMessage == null)
            {
                await _api.SendTextMessageAsync(message.Chat.Id,
                    "Ответьте пользователю, статистику которого вы хотите посмотреть.",
                    replyToMessageId: message.MessageId);
                return;
            }

            var user = message.ReplyToMessage.From;

            var result = await _dbRepository.GetStats(user.Id.ToString());

            await _api.SendTextMessageAsync(message.Chat.Id,
                BuildGetStatMessage(user, result),
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Html);

        }

        private void LogCommand(string startCommandText)
        {
            _logger.LogTrace($"{startCommandText} command recieved");
        }

        private async Task ProcessMyStatsCommand(Message message)
        {
            LogCommand(_commandsService.MyStatsCommandText);

            var user = message.From;

            var result = await _dbRepository.GetStats(user.Id.ToString());

            await _api.SendTextMessageAsync(message.Chat.Id,
                BuildGetStatMessage(user, result),
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Html);
        }

        private static string BuildGetStatMessage(User user, Stats result)
        {
            return $"Статистика пользователя {user.GetBeautyName()}{Environment.NewLine}{Environment.NewLine}" +
                            $"Залито картинок: <b>{result.PicCount}</b>{Environment.NewLine}" +
                            $"Получено лайков: <b>{result.GetLikeCount}</b>{Environment.NewLine}" +
                            ((result.SetSelfLikeCount > 0)
                                ? $"Поставлено лайков (себе): <b>{result.SetLikeCount}</b> (<b>{result.SetSelfLikeCount}</b>).{Environment.NewLine}"
                                : $"Поставлено лайков: <b>{result.SetLikeCount}</b>{Environment.NewLine}");
        }
    }
}