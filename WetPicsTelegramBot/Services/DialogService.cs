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
        private readonly ITelegramBotClient _api;

        private User _me;
        

        public DialogService(ITelegramBotClient api, 
                                ILogger<DialogService> logger, 
                                IChatSettings chatSettings,
                                IDbRepository dbRepository, 
                                IPixivSettings pixivSettings,
                                IBaseDialogService baseDialogService,
                                IMessagesService messagesService)
        {
            _api = api;
            _logger = logger;
            _chatSettings = chatSettings;
            _dbRepository = dbRepository;
            _pixivSettings = pixivSettings;
            _baseDialogService = baseDialogService;
            _messagesService = messagesService;

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

                if (command == _messagesService.DeactivatePhotoRepostCommandText)
                {
                    await ProcessDeactivatePhotoRepostCommand(message);
                }
                else if (command == _messagesService.ActivatePhotoRepostCommandText)
                {
                    await ProcessActivatePhotoRepostCommand(message);
                }
                else if (command == _messagesService.ActivatePhotoRepostHelpCommandText)
                {
                    await ProcessActivatePhotoRepostHelpCommand(message);
                }
                else if (command == _messagesService.MyStatsCommandText)
                {
                    await ProcessMyStatsCommand(message);
                }
                else if (command == _messagesService.StatsCommandText)
                {
                    await ProcessStatsCommand(message);
                }
                else if (command == _messagesService.ActivatePixivCommandText)
                {
                    await ActivatePixivCommand(message);
                }
                else if (command == _messagesService.DeactivatePixivCommandText)
                {
                    await DeactivatePixivCommand(message);
                }

                // if reply to me
                else if (message.ReplyToMessage?.From != null 
                            && (string) message.ReplyToMessage.From.Id == (string) me.Id)
                {
                    // if reply to activateRepost command
                    if (message.ReplyToMessage?.Text.StartsWith(_messagesService.ActivateRepostMessage.Substring(0, 15)) ?? false)
                    {
                        await ProcessReplyToActivatePhotoRepostCommand(message);
                    }
                    else if ((bool) message.ReplyToMessage?.Text.StartsWith(_messagesService.SelectPixivModeMessage))
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

            await _pixivSettings.Add(message.Chat.Id.ToLong(), myStatus, interval);

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
            LogCommand(_messagesService.DeactivatePixivCommandText);
            
            await _pixivSettings.Remove(message.Chat.Id.ToLong());

            await _api.SendTextMessageAsync(message.Chat.Id, "Пиксив деактивирован!");
        }

        private async Task ActivatePixivCommand(Message message)
        {
            LogCommand(_messagesService.ActivatePixivCommandText);

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
            LogCommand(_messagesService.StatsCommandText);

            if (message.ReplyToMessage == null)
            {
                await _api.SendTextMessageAsync(message.Chat.Id,
                    "Ответьте пользователю, статистику которого вы хотите посмотреть.",
                    replyToMessageId: message.MessageId);
                return;
            }

            var user = message.ReplyToMessage.From;

            var result = await _dbRepository.GetStats((string)user.Id);

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
            LogCommand(_messagesService.MyStatsCommandText);

            var user = message.From;

            var result = await _dbRepository.GetStats((string)user.Id);

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

        private async Task ProcessReplyToActivatePhotoRepostCommand(Message message)
        {
            _logger.LogTrace($"reply recieved {message.Text}");

            var fl = message.Text[0];

            switch (fl)
            {
                case '@':
                    await SetRepostId(message.Text, message);
                    break;
                case 'u':
                    await SetRepostId("-" + message.Text.Substring(1), message);
                    break;
                case 'g':
                    await SetRepostId("-" + message.Text.Substring(1), message);
                    break;
                case 'c':
                    await SetRepostId("-100" + message.Text.Substring(1), message);
                    break;
                default:
                    await _api.SendTextMessageAsync(message.Chat.Id,
                        "Неверный формат Id",
                        replyToMessageId: message.MessageId);
                    break;
            }
        }

        private async Task ProcessActivatePhotoRepostHelpCommand(Message message)
        {
            LogCommand(_messagesService.ActivatePhotoRepostHelpCommandText);

            var text = _messagesService.RepostHelpMessage;

            await _api.SendTextMessageAsync(message.Chat.Id,
                text,
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        private async Task ProcessActivatePhotoRepostCommand(Message message)
        {
            LogCommand(_messagesService.ActivatePhotoRepostCommandText);

            var text = _messagesService.ActivateRepostMessage;

            await _api.SendTextMessageAsync(message.Chat.Id,
                text,
                replyMarkup: new ForceReply { Force = true },
                replyToMessageId: message.MessageId);
        }

        private async Task ProcessDeactivatePhotoRepostCommand(Message message)
        {
            LogCommand(_messagesService.DeactivatePhotoRepostCommandText);

            await _chatSettings.Remove((string)message.Chat.Id);

            await _api.SendTextMessageAsync(message.Chat.Id,
                "Пересылка фотографий отключена.",
                replyToMessageId: message.MessageId);
        }

        private async Task SetRepostId(string messageText, Message message)
        {
            try
            {
                var mes = await _api.SendTextMessageAsync(new ChatId(messageText), "Пересылка фотографий настроена.");

                await _chatSettings.Add((string) message.Chat.Id, messageText);

                await _api.SendTextMessageAsync(message.Chat.Id, "Пересылка фотографий включена.");
            }
            catch (Exception e)
            {
                try
                {
                    await _api.SendTextMessageAsync(message.Chat.Id,
                        "Не удается сохранить изменения. Нет доступа к каналу/группе или неверный формат Id.",
                        replyToMessageId: message.MessageId);
                    _logger.LogError($"unable to set repost id" + e.ToString());
                }
                catch (Exception exception)
                {
                    _logger.LogError($"unable to send repost id error" + exception.ToString());
                }
            }
        }
    }
}