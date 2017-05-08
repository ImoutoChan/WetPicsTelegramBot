using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Database;

namespace WetPicsTelegramBot.Services
{
    internal class PhotoPublisherService
    {
        private readonly ILogger<PhotoPublisherService> _logger;
        private readonly ITelegramBotClient _api;
        private readonly IDbRepository _dbRepository;

        private readonly IChatSettings _chatSettings;

        public PhotoPublisherService(ITelegramBotClient api, 
                                        ILogger<PhotoPublisherService> logger, 
                                        IDbRepository dbRepository, 
                                        IChatSettings chatSettings)
        {
            _api = api;
            _logger = logger;
            _dbRepository = dbRepository;
            _chatSettings = chatSettings;

            _api.OnMessage += BotOnMessageReceived;
            _api.OnCallbackQuery += BotOnCallbackQueryReceived;
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;
                if (message == null)
                    return;

                if (message.Type != MessageType.PhotoMessage)
                    return;

                var settings = _chatSettings.Settings.FirstOrDefault(x => x.ChatId == (string)message.Chat.Id);

                if (settings  == null)
                    return;

                var userName = message.From.GetBeautyName();

                var file = await _api.GetFileAsync(message.Photo.LastOrDefault()?.FileId);

                var keyboard = GetPhotoKeyboard(new Vote());

                var mes = await _api.SendPhotoAsync(settings.TargetId,
                    new FileToSend(file.FileId),
                    $"© {userName}",
                    replyMarkup: keyboard);

                await _dbRepository.AddPhoto((string) message.From.Id, (string) mes.Chat.Id, mes.MessageId);
            }
            catch (Exception e)
            {
                _logger.LogError("unable to repost" + e.ToString());
            }
        }

        public async Task PostToChannel(string chatId, string caption, string fileId, string fromUserId)
        {
            var settings = _chatSettings.Settings.FirstOrDefault(x => x.ChatId == chatId);

            if (settings == null)
                return;

            var file = await _api.GetFileAsync(fileId);

            var keyboard = GetPhotoKeyboard(new Vote());

            var mes = await _api.SendPhotoAsync(settings.TargetId,
                new FileToSend(file.FileId),
                $"{caption}",
                replyMarkup: keyboard);

            await _dbRepository.AddPhoto(fromUserId, (string)mes.Chat.Id, mes.MessageId);
        }

        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            CallbackQuery res = null;
            try
            {
                res = callbackQueryEventArgs.CallbackQuery;

                if (!res.Data.StartsWith("vote_"))
                {
                    return;
                }

                var vote = res.Data.Last();
                int score = -1;
                bool? isLiked = null;
                switch (vote)
                {
                    case 'l':
                        isLiked = true;
                        break;
                    case 'd':
                        isLiked = false;
                        break;
                    case 'r':
                        // TODO
                        await _api.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id, "Liked by ...");
                        break;
                    default:
                        if (!Int32.TryParse(vote.ToString(), out score))
                        {
                            return;
                        }
                        break;
                }

                var isChanged = await _dbRepository.AddOrUpdateVote((string) res.From.Id,
                    (string)res.Message.Chat.Id,
                    res.Message.MessageId,
                    isLiked);

                if (isChanged)
                {
                    var votes = await _dbRepository.GetVotes(res.Message.MessageId, (string)res.Message.Chat.Id);

                    var keyboard = GetPhotoKeyboard(votes);

                    await _api.EditMessageReplyMarkupAsync(
                        res.Message.Chat.Id,
                        res.Message.MessageId,
                        keyboard);
                }
                else
                {
                    await _api.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"unable to save vote ({res?.From.Id} : {res?.Message.Chat.Id} : {res?.Message.MessageId})\n{e.Message}");
            }
        }

        private InlineKeyboardMarkup GetPhotoKeyboard(Vote votes)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    new InlineKeyboardButton($"❤️ ({votes.Liked})", "vote_l"),
                    // TODO
                    //new InlineKeyboardButton( $"..?", "vote_r")
                }
            });
        }
    }
}