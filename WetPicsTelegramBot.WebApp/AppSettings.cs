namespace WetPicsTelegramBot.WebApp
{
    public class AppSettings
    {
        public string BotToken { get; set; }
        public string ConnectionString { get; set; }
        public Pixivconfiguration PixivConfiguration { get; set; }
        public SankakuConfiguration SankakuConfiguration { get; set; }
        public DanbooruConfiguration DanbooruConfiguration { get; set; }

        public string WebHookAdress { get; set; }
    }

    public class DanbooruConfiguration
    {
        public string Login { get; set; }

        public string ApiKey { get; set; }

        public int Delay { get; set; }
    }

    public class SankakuConfiguration
    {
        public string Login { get; set; }

        public string PassHash { get; set; }

        public int Delay { get; set; }
    }

    public class Pixivconfiguration
    {
        public string Login { get; set; }

        public string Password { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}