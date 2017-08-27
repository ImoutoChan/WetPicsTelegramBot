using System;
using WetPicsTelegramBot.Services;

namespace WetPicsTelegramBot
{
    class MessagesService : IMessagesService
    {
        public string SelectPixivModeMessage => "Выберете режим";

        public string SelectPixivIntervalMessage => "Введите время в минутах, через которое будут поститься изображения.";

        public string HelpCommandText => "/help";

        public string StartCommandText => "/start";

        public string DeactivatePhotoRepostCommandText => "/deactivatePhotoRepost";

        public string ActivatePhotoRepostCommandText => "/activatePhotoRepost";

        public string ActivatePhotoRepostHelpCommandText => "/activatePhotoRepostHelp";

        public string MyStatsCommandText => "/mystats";

        public string StatsCommandText => "/stats";

        public string ActivatePixivCommandText => "/pixivon";

        public string DeactivatePixivCommandText => "/pixivoff";

        public string RepostHelpMessage =>  $"Id может начинаться с @ для публичных каналов/чатов с заданным username. Для определения Id приватных получателей перейдите в web клиент, выберете нужного получателя.{Environment.NewLine}{Environment.NewLine}" +
                                            $"Вы увидете ссылки вида:{Environment.NewLine}{Environment.NewLine}" +
                                            $"web.telegram org/#/ im?p=<b>с00000000</b>_00000000000000000 для канала,{Environment.NewLine}" +
                                            $"web.telegram org/#/ im?p=<b>g00000000</b> для группы.{Environment.NewLine}{Environment.NewLine}" +
                                            $"Выделенная жирным часть и будет являться Id.";

        public string HelpMessage => $"Список доступных комманд:{Environment.NewLine}{Environment.NewLine}" +
                                     $"{ActivatePhotoRepostCommandText} — включает репост фотографий из данного чата в выбранный канал или группу{Environment.NewLine}" +
                                     $"{DeactivatePhotoRepostCommandText} — отключает репост фотографий из данного чата{Environment.NewLine}" +
                                     $"{StatsCommandText} — показывает статистику пользователя, которому вы отвечаете этим сообщением{Environment.NewLine}" +
                                     $"{MyStatsCommandText} — показывает вашу статистику{Environment.NewLine}" +
                                     $"{ActivatePixivCommandText} — активирует автопосты из пиксива{Environment.NewLine}" +
                                     $"{DeactivatePixivCommandText} — деактивирует автопосты из пиксива{Environment.NewLine}";

        public string ActivateRepostMessage =>  $"Введите Id канала/чата для репоста. Для корректной работы, бот должен быть администратором канала, либо должен состоять в выбранной группе.{Environment.NewLine}" +
                                                $"Пример Id: @channelName u00000000 с00000000 g00000000{Environment.NewLine}" +
                                                $"Справка: {ActivatePhotoRepostHelpCommandText}";

    }
}