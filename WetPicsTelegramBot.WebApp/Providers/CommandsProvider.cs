using WetPicsTelegramBot.WebApp.Providers.Abstract;

namespace WetPicsTelegramBot.WebApp.Providers
{
    class CommandsProvider : ICommandsProvider
    {
        public string HelpCommandText => "/help";

        public string StartCommandText => "/start";

        public string DeactivatePhotoRepostCommandText => "/repostoff";

        public string ActivatePhotoRepostCommandText => "/reposton";

        public string ActivatePhotoRepostHelpCommandText => "/reposthelp";

        public string MyStatsCommandText => "/mystats";

        public string StatsCommandText => "/stats";

        public string IgnoreCommand => "/ignore";

        public string AltIgnoreCommand => "/игнор";

        public string TopCommandText => "/top";

        public string MyTopCommandText => "/mytop";

        public string GlobalTopCommandText => "/globaltop";

        public string SearchIqdbCommandText => "/search";

        public string GetTagsCommandText => "/tags";

        public string TopUsersCommandText => "/topusers";

        public string ChangeLogCommandText => "/changelog";

        public string WetpicsOn => "/wetpicson";

        public string AddImageSourceCommandText => "/addimagesource";

        public string ListImageSourcesCommandText => "/listimagesources";

        public string RemoveImageSourceCommandText => "/removeimagesource";

        public string WetpicsOff => "/wetpicsoff";
    }
}