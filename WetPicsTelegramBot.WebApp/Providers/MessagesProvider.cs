using System;
using Microsoft.Extensions.PlatformAbstractions;
using Telegram.Bot.Types.Enums;
using WetPicsTelegramBot.WebApp.Models;
using WetPicsTelegramBot.WebApp.Providers.Abstract;

namespace WetPicsTelegramBot.WebApp.Providers
{
    class MessagesProvider : IMessagesProvider
    {
        private readonly string _nl = Environment.NewLine;

        private readonly ICommandsProvider _commands;

        public MessagesProvider(ICommandsProvider commands)
        {
            _commands = commands;
        }

        public string SelectPixivModeMessage => "Выберете режим";

        public string SelectPixivIntervalMessageF => $"Выбран режим: {{0}}{_nl}Введите время в минутах, через которое будут поститься изображения.";
        
        public string RepostHelpMessage =>  $"Id может начинаться с @ для публичных каналов/чатов с заданным username. Для определения Id приватных получателей перейдите в web клиент, выберете нужного получателя.{_nl}{_nl}" +
                                            $"Вы увидете ссылки вида:{_nl}{_nl}" +
                                            $"Для канала: web.telegram org/#/ im?p=<b>с00000000</b>_00000000000000000{_nl}" +
                                            $"Для группы: web.telegram org/#/ im?p=<b>g00000000</b>{_nl}{_nl}" +
                                            $"Выделенная жирным часть и будет являться Id.";

        public ReplyMessage HelpMessage 
            => new ReplyMessage($"{PlatformServices.Default.Application.ApplicationName} | Версия: {PlatformServices.Default.Application.ApplicationVersion}{_nl}{_nl}" +
                                $"Список доступных комманд:{_nl}{_nl}" +
                                $"{_commands.ActivatePhotoRepostCommandText} — включает репост изображений из данного чата в выбранный канал или группу{_nl}" +
                                $"{_commands.DeactivatePhotoRepostCommandText} — отключает репост изоражений из данного чата{_nl}" +
                                $"{_commands.StatsCommandText} — показывает статистику пользователя, на сообщение которого вы отвечаете этой командой{_nl}" +
                                $"{_commands.MyStatsCommandText} — показывает вашу статистику{_nl}" +
                                $"{_commands.ActivatePixivCommandText} — активирует постинг изображений из пиксива{_nl}" +
                                $"{_commands.DeactivatePixivCommandText} — деактивирует постинг изображений из пиксива{_nl}" +
                                $"{_commands.IgnoreCommand} ({_commands.AltIgnoreCommand}) — если комманда добавлена в начало описания изображения, то при включенном репосте оно будет проигнорированно{_nl}" +
                                $"{_commands.TopCommandText} — посмотреть топ постов пользователя{_nl}" +
                                $"{_commands.MyTopCommandText} — посмотреть топ ваших постов{_nl}" +
                                $"{_commands.GlobalTopCommandText} — посмотреть топ среди постов всех пользователей{_nl}" +
                                $"{_commands.TopUsersCommandText} — посмотреть топ пользователей{_nl}" +
                                $"{_commands.SearchIqdbCommandText} — искать изображение на iqdb{_nl}" +
                                $"{_commands.GetTagsCommandText} — искать теги для изображения на iqdb{_nl}" +
                                $"{_commands.ChangeLogCommandText} — вывести историю изменений" +
                                $"{_nl}{_nl}" +
                                $"Для комманд топов доступны параметры: -p|period:{{d|day,m|month,y|year}} -c|count:{{количество}} -album{_nl}" +
                                $"Например {_commands.TopCommandText} -p:d -c:6 -album",
                            ParseMode.Markdown);

        public string ActivateRepostMessage =>  $"Введите Id канала, группы или пользователя для репоста. Для корректной работы, бот должен быть администратором канала, либо должен состоять в выбранной группе.{_nl}" +
                                                $"Форматы Id: <code>@channelName</code> <code>u00000000</code> <code>с00000000</code> <code>g00000000</code>{_nl}" +
                                                $"Подробнее: {_commands.ActivatePhotoRepostHelpCommandText}";

        public string DeactivatePhotoRepostMessage => "Пересылка изоражений отключена.";

        public string RepostWrongIdFormat => "Неверный формат Id.";

        public string RepostActivateTargetSuccess => "Пересылка изоражений настроена.";

        public string RepostActivateSourceSuccess => "Пересылка изображений включена.";

        public string RepostActivateSourceFailure =>
            "Не удается сохранить изменения. Нет доступа к каналу/группе или неверный формат Id.";

        public string StatsReplyToUser => 
            "Ответьте пользователю, статистику которого вы хотите посмотреть.";

        public string TopReplyToUser =>
            "Ответьте пользователю, топ постов которого вы хотите посмотреть.";

        public string StatsResultF => $"Статистика пользователя {{0}}{_nl}{_nl}" +
                                      $"Залито картинок: <b>{{1}}</b>{_nl}" +
                                      $"Получено лайков: <b>{{2}}</b>{_nl}" +
                                      $"Поставлено лайков (себе): <b>{{3}}</b> (<b>{{4}}</b>).";

        public string PixivWasDeactivated => "Пиксив деактивирован.";

        public string PixivIncorrectMode => "Выбран некорректный режим.";

        public string PixivIncorrectInterval => "Введен некорректный интервал";

        public string PixivWasActivated => "Пиксив активирован!";

        public ReplyMessage ReplyToImage 
            => new ReplyMessage("Ответьте на сообщение с изображением.");

        public string IqdbNotFound => "К сожалению, похожие изображения не найдены.";

        public ReplyMessage ChangeLogMessage
            => new ReplyMessage($"<b>1.16.0</b>{_nl}" +
                                $"* Добавлен постинг итогов месяца.{_nl}{_nl}" +

                                $"<b>1.15.1</b>{_nl}" +
                                $"* Переписана часть внутренней логики.{_nl}" +
                                $"* Добавлен флаг <code>album</code> ко всем методам топов, который постит альбом со всеми изображениями из текущего топа.{_nl}" +
                                $"* Альбом теперь так же постится и в дневных топах.{_nl}" +
                                $"* Добавлена проверка на права админа при установке и удалении настроек репостов.{_nl}{_nl}" +

                                $"<b>1.14.1</b>{_nl}" +
                                $"* Убран меншен из репостов изображений.{_nl}{_nl}" +

                                $"<b>1.14.0</b>{_nl}" +
                                $"* Увеличено количество изображений в топах до 8.{_nl}" +
                                $"* Исправлена ошибка, когда топы не постили кнопки после восьмой.{_nl}{_nl}" +

                                $"<b>1.13.0</b>{_nl}" +
                                $"* Пользователь бота теперь учавствует во всех общих топах{_nl}{_nl}" +

                                $"<b>1.12.2</b>{_nl}" +
                                $"* Исправлена ошибка с большими изображениями из пиксива{_nl}{_nl}" +

                                $"<b>1.12.0-final</b>{_nl}" +
                                $"* {_commands.IgnoreCommand} теперь не зависит от регистра и имеет русскую альтернативу {_commands.AltIgnoreCommand} для тех, кому лень переключать раскладку;{_nl}" +
                                $"* исправлен баг с изображениями пиксива, когда они весили больше 5 мб и телеграм отказывался их кушать;{_nl}" +
                                $"* в список тегов добавлен добавлен вывод рейтинга изображения (safe, questionable, explicit).",
                            ParseMode.Html);

        public string RepostActivateTargetRestrict => "У пользователя должны быть права админа в целевом чате/канале.";
    }
}