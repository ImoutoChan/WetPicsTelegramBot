using System;

namespace WetPicsTelegramBot
{
    class Messages
    {
        private string _repostHelpMessage = $"Id может начинаться с @ для публичных каналов/чатов с заданным username. Для определения Id приватных получателей перейдите в web клиент, выберете нужного получателя.{Environment.NewLine}{Environment.NewLine}" +
                                            $"Вы увидете ссылки вида:{Environment.NewLine}{Environment.NewLine}" +
                                            $"web.telegram org/#/ im?p=<b>с00000000</b>_00000000000000000 для канала,{Environment.NewLine}" +
                                            $"web.telegram org/#/ im?p=<b>g00000000</b> для группы.{Environment.NewLine}{Environment.NewLine}" +
                                            $"Выделенная жирным часть и будет являться Id.";

        private string _helpMessage = $"Список доступных комманд:{Environment.NewLine}{Environment.NewLine}" +
                                      $"/activatePhotoRepost — включает репост фотографий из данного чата в выбранный канал или группу{Environment.NewLine}" +
                                      $"/deactivatePhotoRepost — отключает репост фотографий из данного чата{Environment.NewLine}" +
                                      $"/stats — показывает статистику пользователя, которому вы отвечаете этим сообщением{Environment.NewLine}" +
                                      $"/mystats — показывает вашу статистику";

        private string _activateRepostMessage = $"Введите Id канала/чата для репоста. Для корректной работы, бот должен быть администратором канала, либо должен состоять в выбранной группе.{Environment.NewLine}" +
                                                $"Пример Id: @channelName u00000000 с00000000 g00000000{Environment.NewLine}" +
                                                $"Справка: /activatePhotoRepostHelp";

        public Messages()
        {
        }

        public string RepostHelpMessage => _repostHelpMessage;

        public string HelpMessage => _helpMessage;

        public string ActivateRepostMessage => _activateRepostMessage;
    }
}