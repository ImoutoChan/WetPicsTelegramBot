using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Helpers;
using WetPicsTelegramBot.Services.Abstract;

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

                if (message.Type != MessageType.PhotoMessage || message.Caption?.Contains("/ignore") == true)
                    return;
                
                _logger.LogTrace("Photo message received");

                var settings = _chatSettings.Settings.FirstOrDefault(x => Int64.Parse(x.ChatId) == message.Chat.Id);

                if (settings == null)
                {
                    _logger.LogTrace("Settings not found.");
                    return;
                }

                var userName = message.From.GetBeautyName();


                var file = await _api.GetFileAsync(message.Photo.LastOrDefault()?.FileId);
                _logger.LogTrace("File was gotten.");

                var keyboard = GetPhotoKeyboard(0);

                var mes = await _api.SendPhotoAsync(settings.TargetId,
                    new FileToSend(file.FileId),
                    $"© {userName}",
                    replyMarkup: keyboard);
                _logger.LogTrace("Photo was send.");

                await _dbRepository.AddPhoto(message.From.Id, mes.Chat.Id, mes.MessageId);
                _logger.LogTrace("Photo was saved.");
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception in photo repost method: {e.Message}");
            }
        }

        public async Task PostToChannel(long chatId, string caption, string fileId, int fromUserId)
        {
            var settings = _chatSettings.Settings.FirstOrDefault(x => x.ChatId == chatId.ToString());

            if (settings == null)
                return;

            var file = await _api.GetFileAsync(fileId);

            var keyboard = GetPhotoKeyboard(0);

            var mes = await _api.SendPhotoAsync(settings.TargetId,
                new FileToSend(file.FileId),
                $"{caption}",
                replyMarkup: keyboard);

            await _dbRepository.AddPhoto(fromUserId, mes.Chat.Id, mes.MessageId);
        }

        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {

            CallbackQuery res = null;
            try
            {
                res = callbackQueryEventArgs.CallbackQuery;
                _logger.LogDebug($"Callback query received ({res?.From.Id} : {res?.Message.Chat.Id} : {res?.Message.MessageId}) : {res?.Data}");

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
                _logger.LogDebug($"Callback query|isLiked: {isLiked}");
                _logger.LogDebug($"Callback query|to db (fromId: {res.From.Id} chatId: {res.Message.Chat.Id} messageId: {res.Message.MessageId} isLiked: {isLiked})");

                var isChanged = await _dbRepository.AddOrUpdateVote(res.From.Id,
                    res.Message.Chat.Id,
                    res.Message.MessageId,
                    isLiked);
                _logger.LogDebug($"Callback query|to db (isChanged: {isChanged})");


                var votes = await _dbRepository.GetVotes(res.Message.MessageId, res.Message.Chat.Id);

                _logger.LogDebug($"Callback query|votes (votes.Liked: {votes.Liked})");

                var keyboard = GetPhotoKeyboard(votes.Liked);

                var counter = 3;
                while (true)
                {
                    try
                    {
                        _logger.LogDebug($"Callback query|update reply (chatId: {res.Message.Chat.Id} messageId: {res.Message.MessageId} counter: {counter})");

                        await _api.EditMessageReplyMarkupAsync(
                            res.Message.Chat.Id,
                            res.Message.MessageId,
                            keyboard);
                        break;
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("Request timed out") && counter > 0)
                        {
                            counter--;
                            continue;
                        }

                        _logger.LogError(
                            $"Callback query|EditMessageReplyMarkupAsync can't update (chatId: {res.Message.Chat.Id} messageId: {res.Message.MessageId})\n" +
                            e.Message);
                        await _api.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception in voting mthod ({res?.From.Id} : {res?.Message.Chat.Id} : {res?.Message.MessageId})\n{e.Message}");
            }
        }

        private InlineKeyboardMarkup GetPhotoKeyboard(int likedCount)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    new InlineKeyboardCallbackButton($"❤️ ({likedCount})", "vote_l"),
                    // TODO
                    //new InlineKeyboardButton( $"..?", "vote_r")
                }
            });
        }
    }
}