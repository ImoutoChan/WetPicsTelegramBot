using WetPicsTelegramBot.WebApp.Models;

namespace WetPicsTelegramBot.WebApp.Providers.Abstract
{
    public interface IMessagesProvider
    {
        ReplyMessage ActivateRepostMessage { get; }

        ReplyMessage HelpMessage { get; }

        ReplyMessage RepostHelpMessage { get; }

        string SelectPixivModeMessage { get; }

        string DeactivatePhotoRepostMessage { get; }
        string RepostWrongIdFormat { get; }
        string RepostActivateTargetSuccess { get; }
        string RepostActivateSourceSuccess { get; }
        string RepostActivateSourceFailure { get; }
        string StatsReplyToUser { get; }
        string TopReplyToUser { get; }

        ReplyMessage StatsResultF { get; }

        string PixivIncorrectMode { get; }
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
    }
}