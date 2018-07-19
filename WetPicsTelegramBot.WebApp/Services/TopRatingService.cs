using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Models;
using WetPicsTelegramBot.WebApp.Services.Abstract;
using File = Telegram.Bot.Types.File;
using FileType = WetPicsTelegramBot.WebApp.Models.FileType;

namespace WetPicsTelegramBot.WebApp.Services
{
    class TopRatingService : ITopRatingService
    {
        private readonly IDbRepository _dbRepository;
        private readonly ITgClient _tgClient;
        private readonly ITopImageDrawService _topImageDrawService;

        public TopRatingService(IDbRepository dbRepository, 
                                ITgClient tgClient,
                                ITopImageDrawService topImageDrawService)
        {
            _dbRepository = dbRepository;
            _tgClient = tgClient;
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
            
            var results = await _dbRepository.GetTopImagesSlow(user?.Id, count, GetFrom(period));

            messageText.AppendLine();

            int counter = 1;
            var fileStreams = new List<MemoryStream>();
            var fileInfos = new List<(File File, FileType FileType)>();
            var keyboardInfo = new List<(long ChatId, int MessageId, string Counter)>();
            foreach (var topResult in results)
            {
                var (photo, info, fileType) = await LoadPhoto(topResult.Photo);
                if (photo == null)
                {
                    continue;
                }

                fileStreams.Add(photo);
                fileInfos.Add((info, fileType));

                var id = counter++;
                var animatedTag = GetAnimatedLabel(fileType);

                messageText.AppendLine($"{id}. Лайков: <b>{topResult.Likes}</b> © {topResult.User.GetBeautyName()}{animatedTag}");
                keyboardInfo.Add((topResult.Photo.ChatId, topResult.Photo.MessageId, id.ToString()));
            }

            if (fileStreams.Any())
            {
                messageText.AppendLine();
                messageText.AppendLine("Нажав на одну из кнопок выше, вы можете запросить форвард изображения.");

                var stream = await Task.Run(
                    () => _topImageDrawService.DrawTopImage(fileStreams.Cast<Stream>().ToList()));
                stream.Seek(0, SeekOrigin.Begin);

                var inlineKeyboardMarkup = GetReplyKeyboardMarkup(keyboardInfo);

                var photoMessage = await _tgClient.Client.SendPhotoAsync(chatId,
                                                                         new InputOnlineFile(stream),
                                                                         imageCaption,
                                                                         replyToMessageId: messageId ?? 0,
                                                                         replyMarkup: inlineKeyboardMarkup);

                if (withAlbum)
                {
                    var albumFiles = fileInfos
                       .Where(x => x.FileType == FileType.Photo)
                       .Select(x => new InputMediaPhoto {Media = new InputMedia(x.File.FileId)})
                       .ToList();

                    if (albumFiles.Any())
                    {
                        await _tgClient.Client.SendMediaGroupAsync(chatId, albumFiles);
                    }
                }

                await _tgClient.Reply(photoMessage, messageText.ToString(), CancellationToken.None,  ParseMode.Html);

                stream.Dispose();
            }
            else
            {
                await _tgClient.Client.SendTextMessageAsync(chatId,
                                                            messageText.ToString(),
                                                            cancellationToken: CancellationToken.None,
                                                            parseMode: ParseMode.Html);
            }

            fileStreams.ForEach(x => x.Dispose());
        }

        private static string GetAnimatedLabel(FileType fileType)
        {
            string animatedTag;
            switch (fileType)
            {
                case FileType.Gif:
                    animatedTag = " <i>animated</i>";
                    break;
                case FileType.Video:
                    animatedTag = " <i>video</i>";
                    break;
                default:
                case FileType.Photo:
                case FileType.None:
                    animatedTag = String.Empty;
                    break;
            }

            return animatedTag;
        }

        public async Task PostUsersTop(ChatId chatId, 
                                       int? messageId, 
                                       int count, 
                                       TopPeriod period)
        {
            var results = await _dbRepository.GetTopUsersSlow(count, GetFrom(period));

            var sb = new StringBuilder();
            sb.Append("Топ юзеров");
            sb.Append(GetPeriodString(period));

            sb.AppendLine();
            sb.AppendLine();

            int counter = 1;
            foreach (var topEntry in results)
            {
                sb.AppendLine($"{counter++}. {topEntry.User.GetBeautyName()} " +
                              $"/ Изображений: <b>{topEntry.Photos}</b> " +
                              $"/ Лайков: <b>{topEntry.Likes}</b>");
            }

            await _tgClient.Client.SendTextMessageAsync(chatId, 
                                                        sb.ToString(), 
                                                        ParseMode.Html, 
                                                        replyToMessageId: messageId ?? 0);
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

        private async Task<(MemoryStream Stream, File Info, FileType FileType)> LoadPhoto(Photo topPhoto)
        {
            var photoMessage = new Message {MessageId = topPhoto.MessageId, Chat = new Chat {Id = topPhoto.ChatId}};

            var replyToPhotomessage = await _tgClient.Reply(photoMessage, "Сорян, грузим пикчу...", CancellationToken.None);

            var fileType = FileType.Photo;
            try
            {
                var photos = replyToPhotomessage.ReplyToMessage.Photo;

                string fileId;
                if (photos?.Any() == true)
                {
                    var photo = photos.Last();
                    fileId = photo.FileId;
                }
                else
                {
                    if (replyToPhotomessage.ReplyToMessage.Document != null)
                    {
                        fileId = replyToPhotomessage.ReplyToMessage.Document.Thumb.FileId;
                        fileType = FileType.Gif;
                    }
                    else if (replyToPhotomessage.ReplyToMessage.Video != null)
                    {
                        fileId = replyToPhotomessage.ReplyToMessage.Video.Thumb.FileId;
                        fileType = FileType.Video;
                    }
                    else
                    {
                        return (null, null, FileType.None);
                    }
                }


                var ms = new MemoryStream();
                var photoInfo = await _tgClient.Client.GetInfoAndDownloadFileAsync(fileId, ms);
                ms.Seek(0, SeekOrigin.Begin);
                return (Stream: ms, Info: photoInfo, FileType: fileType);
            }
            finally
            {
                await _tgClient.Client.DeleteMessageAsync(replyToPhotomessage.Chat.Id, replyToPhotomessage.MessageId);
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
            string periodString;
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

        private InlineKeyboardMarkup GetReplyKeyboardMarkup(List<(long ChatId, int MessageId, string Counter)> messages)
        {
            var buttons = messages
                .Select(x => InlineKeyboardButton
                           .WithCallbackData($"{x.Counter}", 
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