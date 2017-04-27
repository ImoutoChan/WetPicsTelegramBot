using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Database;

namespace WetPicsTelegramBot
{
    internal class PhotoPublisherService
    {
        private readonly ILogger<PhotoPublisherService> _logger;
        private readonly ITelegramBotClient _api;
        private readonly IDbRepository _dbRepository;

        private readonly IChatSettings _chatSettings;

        public PhotoPublisherService(ITelegramBotClient api, ILogger<PhotoPublisherService> logger, IDbRepository dbRepository, IChatSettings chatSettings)
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

                var userName = message.GetBeautyName();

                var file = await _api.GetFileAsync(message.Photo.LastOrDefault()?.FileId);

                var keyboard = GetPhotoKeyboard(new Vote());

                var mes = await _api.SendPhotoAsync(settings.TargetId,
                    new FileToSend(file.FileId),
                    $"© {userName}.",
                    replyMarkup: keyboard);

                await _dbRepository.AddPhoto((string) message.From.Id, settings.TargetId, mes.MessageId);
            }
            catch (Exception e)
            {
                _logger.LogError("unable to repost" + e.ToString());
            }
        }

        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            try
            {
                var res = callbackQueryEventArgs.CallbackQuery;

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
                    default:
                        if (!Int32.TryParse(vote.ToString(), out score))
                        {
                            return;
                        }
                        break;
                }

                await _dbRepository.AddOrUpdateVote((string) res.From.Id,
                    (string)res.Message.Chat.Id,
                    res.Message.MessageId,
                    score > 0 ? score : (int?) null,
                    isLiked);

                var votes = await _dbRepository.GetVotes(res.Message.MessageId);

                var keyboard = GetPhotoKeyboard(votes);

                await _api.EditMessageReplyMarkupAsync(
                    res.Message.Chat.Id,
                    res.Message.MessageId,
                    keyboard
                );
            }
            catch (Exception e)
            {
                _logger.LogError("unable to save vote" + e.ToString());
            }
        }

        private InlineKeyboardMarkup GetPhotoKeyboard(Vote votes)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    new InlineKeyboardButton($"❤️ ({votes.Liked})", "vote_l"),
                    new InlineKeyboardButton($"❌ ({votes.Disliked})", "vote_d"),
                }
            });
        }
    }
}