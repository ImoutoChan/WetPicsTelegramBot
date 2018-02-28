using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Database.Model;
using WetPicsTelegramBot.Helpers;
using WetPicsTelegramBot.Models;
using WetPicsTelegramBot.Services.Abstract;
using File = Telegram.Bot.Types.File;

namespace WetPicsTelegramBot.Services
{
    class TopRatingService : ITopRatingService
    {
        private readonly IDbRepository _dbRepository;
        private readonly ITelegramBotClient _telegramApi;
        private readonly ITopImageDrawService _topImageDrawService;

        public TopRatingService(IDbRepository dbRepository, 
                                ITelegramBotClient telegramApi,
                                ITopImageDrawService topImageDrawService)
        {
            _dbRepository = dbRepository;
            _telegramApi = telegramApi;
            _topImageDrawService = topImageDrawService;
        }


        public async Task PostTop(ChatId chatId, 
                                  int? messageId, 
                                  TopSource topSource = TopSource.Reply,
                                  int count = 5,
                                  TopPeriod period = TopPeriod.AllTime,
                                  User user = null,
                                  bool withAlbum = false)
        {
            var imageCaption = GetTitleString(topSource, user) + GetPeriodString(period);

            var messageText = new StringBuilder();
            
            var results = await _dbRepository.GetTopImagesSlow(user?.Id, count, from: GetFrom(period));

            messageText.AppendLine();

            int counter = 1;
            foreach (var topEntry in results)
            {
                messageText.AppendLine($"{counter++}. Лайков: <b>{topEntry.Likes}</b> © {topEntry.User.GetBeautyName()}");
            }

            messageText.AppendLine();
            messageText.AppendLine("Нажав на одну из кнопок выше, вы можете запросить форвард изображения.");

            var topPhotots = results.Select(x => x.Photo).ToList();

            var fileStreams = new List<MemoryStream>();
            var fileInfos = new List<File>();
            foreach (var topPhoto in topPhotots)
            {
                var (photo, info) = await LoadPhoto(topPhoto);
                if (photo == null)
                {
                    continue;
                }

                fileStreams.Add(photo);
                fileInfos.Add(info);
            }

            if (fileStreams.Any())
            {

                var stream = await Task.Run(
                    () => _topImageDrawService.DrawTopImage(fileStreams.Cast<Stream>().ToList()));
                stream.Seek(0, SeekOrigin.Begin);

                var inlineKeyboardMarkup = GetReplyKeyboardMarkup(topPhotots.Select(x => (x.ChatId, x.MessageId)));

                var photoMessage = await _telegramApi.SendPhotoAsync(chatId,
                                                                      new InputOnlineFile(stream),
                                                                      imageCaption,
                                                                      replyToMessageId: messageId ?? 0,
                                                                      replyMarkup: inlineKeyboardMarkup);

                if (withAlbum)
                {
                    await _telegramApi.SendMediaGroupAsync(
                        chatId, 
                        fileInfos.Select(x => new InputMediaPhoto {Media = new InputMedia(x.FileId)}));
                }

                await _telegramApi.Reply(photoMessage, messageText.ToString(), ParseMode.Html);

                stream.Dispose();
            }

            fileStreams.ForEach(x => x.Dispose());
        }

        public async Task PostUsersTop(ChatId chatId, 
                                       int? messageId, 
                                       int count, 
                                       TopPeriod period)
        {
            var results = await _dbRepository.GetTopUsersSlow(count, from: GetFrom(period));

            var sb = new StringBuilder();
            sb.Append("Топ юзеров");
            sb.Append(GetPeriodString(period));

            sb.AppendLine();
            sb.AppendLine();

            int counter = 1;
            foreach (var topEntry in results)
            {
                sb.AppendLine($"{counter++}. {topEntry.User.GetBeautyName()} | Изображений: <b>{topEntry.Photos}</b> | Лайков: <b>{topEntry.Likes}</b>");
            }

            await _telegramApi.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Html, replyToMessageId: messageId ?? 0);
        }


        private DateTimeOffset GetFrom(TopPeriod period)
        {
            switch (period)
            {
                default:
                case TopPeriod.AllTime:
                    return default(DateTimeOffset);
                case TopPeriod.Day:
                    return DateTimeOffset.Now.AddDays(-1);
                case TopPeriod.Month:
                    return DateTimeOffset.Now.AddMonths(-1);
                case TopPeriod.Year:
                    return DateTimeOffset.Now.AddYears(-1);
            }
        }

        private async Task<(MemoryStream Stream, File Info)> LoadPhoto(Photo topPhoto)
        {
            var photoMessage = new Message {MessageId = topPhoto.MessageId, Chat = new Chat {Id = topPhoto.ChatId}};

            var replyToPhotomessage = await _telegramApi.Reply(photoMessage, "Сорян, грузим пикчу...");

            try
            {
                var photos = replyToPhotomessage.ReplyToMessage.Photo;

                if (!photos.Any())
                {
                    return (null, null);
                }

                var photo = photos.Last();

                var ms = new MemoryStream();
                var photoInfo = await _telegramApi.GetInfoAndDownloadFileAsync(photo.FileId, ms);
                ms.Seek(0, SeekOrigin.Begin);
                return (Stream: ms, Info: photoInfo);
            }
            finally
            {
                await _telegramApi.DeleteMessageAsync(replyToPhotomessage.Chat.Id, replyToPhotomessage.MessageId);
            }
        }

        private string GetTitleString(TopSource topSource, User user)
        {
            switch (topSource)
            {
                case TopSource.Reply:
                    return $"Топ пользователя {user.GetBeautyName()}";
                case TopSource.My:
                    return $"Топ пользователя {user.GetBeautyName()}";
                default:
                case TopSource.Global:
                    return $"Топ среди всех пользователей";
            }
        }

        private static string GetPeriodString(TopPeriod period)
        {
            var periodString = String.Empty;
            switch (period)
            {
                default:
                case TopPeriod.AllTime:
                    periodString = " за все время.";
                    break;
                case TopPeriod.Day:
                    periodString = " за день.";
                    break;
                case TopPeriod.Month:
                    periodString = " за месяц.";
                    break;
                case TopPeriod.Year:
                    periodString = " за год.";
                    break;
            }

            return periodString;
        }

        private InlineKeyboardMarkup GetReplyKeyboardMarkup(
            IEnumerable<(long ChatId, int MessageId)> messagesEnumerable)
        {
            var messages = messagesEnumerable as IList<(long ChatId, int MessageId)> ?? messagesEnumerable.ToList();
            int counter = 0;

            var buttons = messages
                .Select(x => InlineKeyboardButton
                           .WithCallbackData($"{++counter}", 
                                             $"forward_request|chatId#{x.ChatId}_messageId#{x.MessageId}"))
                .ToList();
            
            return new InlineKeyboardMarkup(MakeItCute(buttons));
        }

        /// <summary>
        /// Split buttons by 8 in each row
        /// </summary>
        private InlineKeyboardButton[][] MakeItCute(List<InlineKeyboardButton> buttons)
        {
            var rows = buttons.Count / 8;
            var lastColumns = buttons.Count % 8;
            if (lastColumns > 0)
            {
                rows++;
            }

            var result = new InlineKeyboardButton[rows][];
            for (int i = 0; i < rows; i++)
            {
                result[i] = buttons.Skip(i * 8).Take(8).ToArray();
            }

            return result;
        }
    }
}