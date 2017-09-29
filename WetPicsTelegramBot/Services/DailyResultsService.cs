using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Helpers;
using WetPicsTelegramBot.Services.Abstract;
using WetPicsTelegramBot.Services.Dialog;

namespace WetPicsTelegramBot.Services
{
    class DailyResultsService
    {
        private readonly ILogger<DailyResultsService> _logger;
        private readonly ITopRatingService _topRatingService;
        private readonly IDbRepository _dbRepository;
        private readonly ITelegramBotClient _telegramBotClient;
        private static readonly Random _random = new Random();

        private readonly string[] _dayTypes =
        {
#if DEBUG
            "прекрасный",
#else
            "прекрасный как младшие сестренки"1234,
            "ужасный как грядущая экранизация кино",
            "странный как вкусы Онииичана*",
            "унылый (как обычно)",
            "развратный как местный топ пиксива",
            "длиный как грудь на фоточках Алекса",
            "короткий как длина волос у девочек Наги",
            "мокрый как школьницы после \"дождя\"",
            "влажный как роса на ножках лолей",
            "традиционный как ориентация Хоши",
            "неприступный как мораль Брауни"
#endif
        };

        public DailyResultsService(ILogger<DailyResultsService> logger,
                                   ITopRatingService topRatingService,
                                   IDbRepository dbRepository,
                                   ITelegramBotClient telegramBotClient)
        {
            _logger = logger;
            _topRatingService = topRatingService;
            _dbRepository = dbRepository;
            _telegramBotClient = telegramBotClient;
        }

        public async Task PostDailyResults(long chatId, int messageId)
        {
            _logger.LogTrace("Posting daily results");

            try
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

                await _telegramBotClient.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Html);

                await _topRatingService.PostTop(chatId, messageId, TopSource.Global, 5, TopPeriod.Day);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e, nameof(PostDailyResults));
            }
        }
    }
}
