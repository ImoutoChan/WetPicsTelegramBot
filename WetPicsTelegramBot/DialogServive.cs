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

namespace WetPicsTelegramBot
{
    internal class DialogServive
    {
        private readonly ILogger<DialogServive> _logger;
        private readonly IChatSettings _chatSettings;
        private readonly IDbRepository _dbRepository;
        private readonly ITelegramBotClient _api;

        private User _me;

        private readonly Messages _messages;

        public DialogServive(ITelegramBotClient api, 
                                ILogger<DialogServive> logger, 
                                IChatSettings chatSettings,
                                IDbRepository dbRepository)
        {
            _api = api;
            _logger = logger;
            _chatSettings = chatSettings;
            _dbRepository = dbRepository;
            _messages = new Messages();

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

                var command = message.Text.Split(new [] {"@", " "}, StringSplitOptions.RemoveEmptyEntries).First();
                var me = await GetMe();

                if (command == "/help" || command == "/start")
                {
                    await ProcessHelpCommand(message);
                }
                else if (command == "/deactivatePhotoRepost")
                {
                    await ProcessDeactivatePhotoRepostCommand(message);
                }
                else if (command == "/activatePhotoRepost")
                {
                    await ProcessActivatePhotoRepostCommand(message);
                }
                else if (command == "/activatePhotoRepostHelp")
                {
                    await ProcessActivatePhotoRepostHelpCommand(message);
                }
                else if (command == "/mystats")
                {
                    await ProcessMyStatsCommand(message);
                }
                else if (command == "/stats")
                {
                    await ProcessStatsCommand(message);
                }

                // if reply to me
                else if (me != null 
                    && message.ReplyToMessage?.From != null 
                    && (string) message.ReplyToMessage.From.Id == (string) me.Id)
                {
                    // if reply to activateRepost command
                    if (message.ReplyToMessage?.Text.StartsWith(_messages.ActivateRepostMessage.Substring(0, 15)) ?? false)
                    {
                        await ProcessReplyToActivatePhotoRepostCommand(message);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"unable to process message" + e.ToString());
            }
        }

        private async Task ProcessStatsCommand(Message message)
        {
            _logger.LogTrace("stats command recieved");

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

        private async Task ProcessMyStatsCommand(Message message)
        {
            _logger.LogTrace("mystats command recieved");

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
            _logger.LogTrace("activatePhotoRepostHelp command recieved");
            
            var text = _messages.RepostHelpMessage;

            await _api.SendTextMessageAsync(message.Chat.Id,
                text,
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        private async Task ProcessActivatePhotoRepostCommand(Message message)
        {
            _logger.LogTrace("activatePhotoRepost command recieved");
            
            var text = _messages.ActivateRepostMessage;

            await _api.SendTextMessageAsync(message.Chat.Id,
                text,
                replyMarkup: new ForceReply { Force = true },
                replyToMessageId: message.MessageId);
        }

        private async Task ProcessDeactivatePhotoRepostCommand(Message message)
        {
            _logger.LogTrace("deactivatePhotoRepost command recieved");

            await _chatSettings.Remove((string)message.Chat.Id);

            await _api.SendTextMessageAsync(message.Chat.Id,
                "Пересылка фотографий отключена.",
                replyToMessageId: message.MessageId);
        }

        private async Task ProcessHelpCommand(Message message)
        {
            _logger.LogTrace("help command recieved");
            
            var text = _messages.HelpMessage;

            await _api.SendTextMessageAsync(message.Chat.Id,
                text,
                ParseMode.Markdown,
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