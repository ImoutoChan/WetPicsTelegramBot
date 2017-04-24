using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace WetPicsTelegramBot
{
    public class PhotoPublisherService
    {
        private readonly TelegramBotClient _api;
        private List<ChatSetting> _chatSettings;
        public PhotoPublisherService(TelegramBotClient api)
        {
            _api = api;

            _api.OnMessage += BotOnMessageReceived;
            _api.OnCallbackQuery += BotOnCallbackQueryReceived;

            DbRepository.Instance.ChatSettingsChanged += DbRepositoryOnChatSettingsChanged;
        }

        public async Task Init()
        {
            await ReloadSettings();
        }

        private async void DbRepositoryOnChatSettingsChanged(object sender, EventArgs eventArgs)
        {
            await ReloadSettings();
        }

        private async Task ReloadSettings()
        {
            _chatSettings = await DbRepository.Instance.GetChatSettings();
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

                var settings = _chatSettings.FirstOrDefault(x => x.ChatId == (string)message.Chat.Id);

                if (settings  == null)
                    return;

                var userName = message.GetBeautyName();

                var file = await _api.GetFileAsync(message.Photo.LastOrDefault()?.FileId);

                var keyboard = GetPhotoKeyboard(new Vote());

                var mes = await _api.SendPhotoAsync(settings.TargetId,
                    new FileToSend(file.FileId),
                    $"© {userName}.",
                    replyMarkup: keyboard);

                await DbRepository.Instance.AddPhoto((string) message.From.Id, settings.TargetId, mes.MessageId);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
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

                await DbRepository.Instance.AddOrUpdateVote((string) res.From.Id,
                    (string)res.Message.Chat.Id,
                    res.Message.MessageId,
                    score > 0 ? score : (int?) null,
                    isLiked);

                var votes = await DbRepository.Instance.GetVotes(res.Message.MessageId);

                var keyboard = GetPhotoKeyboard(votes);

                await _api.EditMessageReplyMarkupAsync(
                    res.Message.Chat.Id,
                    res.Message.MessageId,
                    keyboard
                );
            }
            catch (Exception e)
            {
               Debug.WriteLine(e);
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