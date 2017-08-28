using System;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services
{
    class MessagesService : IMessagesService
    {
        private readonly ICommandsService _commands;

        public MessagesService(ICommandsService commands)
        {
            _commands = commands;
        }

        public string SelectPixivModeMessage => "Выберете режим";

        public string SelectPixivIntervalMessage => "Введите время в минутах, через которое будут поститься изображения.";
        
        public string RepostHelpMessage =>  $"Id может начинаться с @ для публичных каналов/чатов с заданным username. Для определения Id приватных получателей перейдите в web клиент, выберете нужного получателя.{Environment.NewLine}{Environment.NewLine}" +
                                            $"Вы увидете ссылки вида:{Environment.NewLine}{Environment.NewLine}" +
                                            $"web.telegram org/#/ im?p=<b>с00000000</b>_00000000000000000 для канала,{Environment.NewLine}" +
                                            $"web.telegram org/#/ im?p=<b>g00000000</b> для группы.{Environment.NewLine}{Environment.NewLine}" +
                                            $"Выделенная жирным часть и будет являться Id.";

        public string HelpMessage => $"Список доступных комманд:{Environment.NewLine}{Environment.NewLine}" +
                                     $"{_commands.ActivatePhotoRepostCommandText} — включает репост изображений из данного чата в выбранный канал или группу{Environment.NewLine}" +
                                     $"{_commands.DeactivatePhotoRepostCommandText} — отключает репост изоражений из данного чата{Environment.NewLine}" +
                                     $"{_commands.StatsCommandText} — показывает статистику пользователя, на сообщение которого, вы отвечаете этой командой{Environment.NewLine}" +
                                     $"{_commands.MyStatsCommandText} — показывает вашу статистику{Environment.NewLine}" +
                                     $"{_commands.ActivatePixivCommandText} — активирует постинг изображений из пиксива{Environment.NewLine}" +
                                     $"{_commands.DeactivatePixivCommandText} — деактивирует постинг изображений из пиксива{Environment.NewLine}" +
                                     $"{_commands.IgnoreCommand} — если комманда добавлена в начало описания изображения, то при включенном репосте оно будет проигнорированно{Environment.NewLine}";

        public string ActivateRepostMessage =>  $"Введите Id канала, группы или пользователя для репоста. Для корректной работы, бот должен быть администратором канала, либо должен состоять в выбранной группе.{Environment.NewLine}" +
                                                $"Форматы Id: @channelName u00000000 с00000000 g00000000{Environment.NewLine}" +
                                                $"Подробнее: {_commands.ActivatePhotoRepostHelpCommandText}";

        public string DeactivatePhotoRepostMessage => "Пересылка изоражений отключена.";

        public string RepostWrongIdFormat => "Неверный формат Id.";

        public string RepostActivateTargetSuccess => "Пересылка изоражений настроена.";

        public string RepostActivateSourceSuccess => "Пересылка изображений включена.";

        public string RepostActivateSourceFailure =>
            "Не удается сохранить изменения. Нет доступа к каналу/группе или неверный формат Id.";

        public string StatsReplyToUser => 
            "Ответьте пользователю, статистику которого вы хотите посмотреть.";

        public string StatsResult => $"Статистика пользователя {{0}}{Environment.NewLine}{Environment.NewLine}" +
                                     $"Залито картинок: <b>{{1}}</b>{Environment.NewLine}" +
                                     $"Получено лайков: <b>{{2}}</b>{Environment.NewLine}" +
                                     $"Поставлено лайков (себе): <b>{{3}}</b> (<b>{{4}}</b>).";

        public string PixivWasDeactivated => "Пиксив деактивирован.";
    }
}