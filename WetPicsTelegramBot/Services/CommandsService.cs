using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services
{
    class CommandsService : ICommandsService
    {
        public string HelpCommandText => "/help";

        public string StartCommandText => "/start";

        public string DeactivatePhotoRepostCommandText => "/repostoff";

        public string ActivatePhotoRepostCommandText => "/reposton";

        public string ActivatePhotoRepostHelpCommandText => "/reposthelp";

        public string MyStatsCommandText => "/mystats";

        public string StatsCommandText => "/stats";

        public string ActivatePixivCommandText => "/pixivon";

        public string DeactivatePixivCommandText => "/pixivoff";

        public string IgnoreCommand => "/ignore";

        public string TopCommandText => "/top";

        public string MyTopCommandText => "/mytop";

        public string GlobalTopCommandText => "/globaltop";

        public string SearchIqdbCommandText => "/search";

        public string GetTagsCommandText => "/tags";
    }
}