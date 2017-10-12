using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Models
{
    class TopUsersEntry
    {
        public int Photos { get; set; }

        public int Likes { get; set; }

        public ChatUser User { get; set; }
    }
}