namespace WetPicsTelegramBot.WebApp.Models.Settings
{
    public class AppSettings
    {
        public string BotToken { get; set; }

        public string ConnectionString { get; set; }

        public PixivConfiguration PixivConfiguration { get; set; }

        public SankakuConfiguration SankakuConfiguration { get; set; }

        public DanbooruConfiguration DanbooruConfiguration { get; set; }

        public string WebHookAddress { get; set; }
    }
}