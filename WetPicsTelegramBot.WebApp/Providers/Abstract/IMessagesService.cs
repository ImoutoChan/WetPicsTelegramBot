using WetPicsTelegramBot.WebApp.Models;

namespace WetPicsTelegramBot.WebApp.Providers.Abstract
{
    public interface IMessagesProvider
    {
        ReplyMessage ActivateRepostMessage { get; }

        ReplyMessage HelpMessage { get; }

        ReplyMessage RepostHelpMessage { get; }

        string SelectPixivIntervalMessageF { get; }
        string SelectPixivModeMessage { get; }
        string DeactivatePhotoRepostMessage { get; }
        string RepostWrongIdFormat { get; }
        string RepostActivateTargetSuccess { get; }
        string RepostActivateSourceSuccess { get; }
        string RepostActivateSourceFailure { get; }
        string StatsReplyToUser { get; }
        string TopReplyToUser { get; }
        string StatsResultF { get; }
        string PixivWasDeactivated { get; }
        string PixivIncorrectMode { get; }
        string PixivIncorrectInterval { get; }
        string PixivWasActivated { get; }

        ReplyMessage ReplyToImage { get; }

        string IqdbNotFound { get; }

        ReplyMessage ChangeLogMessage { get; }

        string RepostActivateTargetRestrict { get; }
    }
}