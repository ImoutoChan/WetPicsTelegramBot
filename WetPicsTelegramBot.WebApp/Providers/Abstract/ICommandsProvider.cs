namespace WetPicsTelegramBot.WebApp.Providers.Abstract
{
    public interface ICommandsProvider
    {
        string ActivatePhotoRepostCommandText { get; }
        string ActivatePhotoRepostHelpCommandText { get; }
        string DeactivatePhotoRepostCommandText { get; }
        string DeactivatePixivCommandText { get; }
        string HelpCommandText { get; }
        string MyStatsCommandText { get; }
        string StartCommandText { get; }
        string StatsCommandText { get; }
        string IgnoreCommand { get; }
        string AltIgnoreCommand { get; }
        string TopCommandText { get; }
        string MyTopCommandText { get; }
        string GlobalTopCommandText { get; }
        string SearchIqdbCommandText { get; }
        string GetTagsCommandText { get; }
        string TopUsersCommandText { get; }
        string ChangeLogCommandText { get; }
        string AddImageSourceCommandText { get; }
        string WetpicsOn { get; }
        string ListImageSourcesCommandText { get; }
        string RemoveImageSourceCommandText { get; }
        string WetpicsOff { get; }
    }
}