using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Models;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services
{
    class ScheduledResultsService : IScheduledResultsService
    {
        private const int PostResultsCount = 10;
        private readonly ILogger<ScheduledResultsService> _logger;
        private readonly ITopRatingService _topRatingService;
        private readonly IDbRepository _dbRepository;
        private readonly ITgClient _tgClient;

        private static readonly Random _random = new Random();

        private readonly string[] _dayTypes =
        {
#if DEBUG
            "прекрасный",
#else
            "прекрасный, как младшие сестренки,",
            "ужасный, как грядущая экранизация кино,",
            "странный, как вкусы Онииичана,",
            "развратный, как местный топ пиксива,",
            "длиный, как грудь на фоточках Алекса,",
            "короткий, как длина волос у девочек Наги,",
            "мокрый, как школьницы после дождя,",
            "влажный, как роса на ножках лолей,",
            "традиционный, как ориентация Хоши,",
            "неприступный, как мораль Брауни,"
#endif
        };

        public ScheduledResultsService(ILogger<ScheduledResultsService> logger,
                                       ITopRatingService topRatingService,
                                       IDbRepository dbRepository,
                                       ITgClient tgClient)
        {
            _logger = logger;
            _topRatingService = topRatingService;
            _dbRepository = dbRepository;
            _tgClient = tgClient;
        }

        public async Task PostResults(
            ChatId chatId,
            ScheduledResultType scheduledResultType)
        {
            try
            {
                _logger.LogInformation($"Posting {scheduledResultType.ToString()} results");

                await PosterSelector(scheduledResultType).Invoke(chatId);
            }
            catch (ApiRequestException e)
                when (e.Message.Contains("Forbidden: bot was blocked by the user"))
            {
                _logger.LogError(e, $"Scheduled results error: bot was kicked from {chatId}");
                await _dbRepository.RemoveRepostSettings(chatId.Identifier);
                _logger.LogInformation($"Chat {chatId} was removed from repost settings.");
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
            }
        }

        private async Task PostDailyResults(ChatId chatId)
        {
            var sb = new StringBuilder();

            var dayRandom = _random.Next(_dayTypes.Length);
            var dayType = _dayTypes[dayRandom];

            sb.AppendLine($"Заканчивается {DateTimeOffset.Now.ToString("dd MMMM", new CultureInfo("RU-ru"))}, {dayType} день.");
            sb.AppendLine($"Давайте же подведем итоги!");

            var stats = await _dbRepository.GetGlobalStats(DateTimeOffset.Now.AddDays(-1));

            sb.AppendLine($"За сегодня было запощено <b>{stats.PicCount}</b> изображений, и <b>{stats.PicAnyLiked}</b> из них даже кому-то понравились. Всего поставленно было <b>{stats.LikesCount}</b> лайков.");
            sb.AppendLine();
            sb.AppendLine("А теперь взглянем на топ.");
            sb.AppendLine("#daily #top");

            await _tgClient.Client.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Html);

            await _topRatingService.PostTop(chatId, null, TopSource.Global, PostResultsCount, TopPeriod.Day, withAlbum: true);

            await _topRatingService.PostUsersTop(chatId, null, 8, TopPeriod.Day);
        }

        private async Task PostMonthlyResults(ChatId chatId)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Вау. Еще минус один месяц.");
            sb.AppendLine("Посмотрим, что у нас там за этот месяц случилось?");

            var stats = await _dbRepository.GetGlobalStats(DateTimeOffset.Now.AddMonths(-1));

            sb.AppendLine($"За месяц у нас было запощено аж <b>{stats.PicCount}</b> пикчей, правда по душе пришлись лишь <b>{stats.PicAnyLiked}</b> из них. Суммарно налепили <b>{stats.LikesCount}</b> лайков. Можно было и побольше.");
            sb.AppendLine();
            sb.AppendLine("Посмотрим на результаты.");
            sb.AppendLine("#monthly #top");

            await _tgClient.Client.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Html);

            await _topRatingService.PostTop(chatId, null, TopSource.Global, PostResultsCount, TopPeriod.Month, withAlbum: true);

            await _topRatingService.PostUsersTop(chatId, null, 8, TopPeriod.Month);
        }

        private async Task PostWeeklyResults(ChatId chatId)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Недельный топ хентайного чата, get your body ready");

            var stats = await _dbRepository.GetGlobalStats(DateTimeOffset.Now.AddDays(-7));

            sb.AppendLine($"За неделю нам отправили <b>{stats.PicCount}</b> изображений, <b>{stats.PicAnyLiked}</b> из них получили лайки. Всего же лайков было <b>{stats.LikesCount}</b>.");
            sb.AppendLine();
            sb.AppendLine("Посмотрим на лучшие картинки за неделю.");
            sb.AppendLine("#weekly #top");

            await _tgClient.Client.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Html);

            await _topRatingService.PostTop(chatId, null, TopSource.Global, PostResultsCount, TopPeriod.Week, withAlbum: true);

            await _topRatingService.PostUsersTop(chatId, null, 8, TopPeriod.Week);
        }

        private Func<ChatId, Task> PosterSelector(
            ScheduledResultType scheduledResultType)
        {
            switch (scheduledResultType)
            {
                case ScheduledResultType.Daily:
                    return PostDailyResults;
                case ScheduledResultType.Weekly:
                    return PostWeeklyResults;
                case ScheduledResultType.Monthly:
                    return PostMonthlyResults;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scheduledResultType), scheduledResultType, null);
            }
        }
    }
}
