using WetPicsTelegramBot.Data.Entities;

namespace WetPicsTelegramBot.Data.Models
{
    public class TopUsersEntry
    {
        public int Photos { get; set; }

        public int Likes { get; set; }

        public ChatUser User { get; set; }
    }
}