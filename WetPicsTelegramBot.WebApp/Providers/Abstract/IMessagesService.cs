using WetPicsTelegramBot.WebApp.Models;

namespace WetPicsTelegramBot.WebApp.Providers.Abstract
{
    public interface IMessagesProvider
    {
        ReplyMessage ActivateRepostMessage { get; }

        ReplyMessage HelpMessage { get; }

        ReplyMessage RepostHelpMessage { get; }

        ReplyMessage SelectModeMessage { get; }

        string DeactivatePhotoRepostMessage { get; }
        string RepostWrongIdFormat { get; }
        string RepostActivateTargetSuccess { get; }
        string RepostActivateSourceSuccess { get; }
        string RepostActivateSourceFailure { get; }
        string StatsReplyToUser { get; }
        string TopReplyToUser { get; }

        ReplyMessage StatsResultF { get; }

        string IncorrectMode { get; }
        string PixivWasActivated { get; }

        ReplyMessage ReplyToImage { get; }

        string IqdbNotFound { get; }

        ReplyMessage ChangeLogMessage { get; }

        string RepostActivateTargetRestrict { get; }

        ReplyMessage SelectImageSource { get; }
        ReplyMessage SelectWetpicsInterval { get; }
        ReplyMessage WetpicsIncorrectInterval { get; }
        ReplyMessage WetpicsWasActivated { get; }
        ReplyMessage WetpicsWasDeactivated { get; }
        ReplyMessage PixivSourceAddSuccess { get; }
        ReplyMessage ZeroSources { get; }
        ReplyMessage RemoveImageSourceSuccess { get; }
        ReplyMessage RemoveImageSourceFail { get; }
        ReplyMessage DanbooruSourceAddSuccess { get; }
        ReplyMessage YandereSourceAddSuccess { get; }
    }
}