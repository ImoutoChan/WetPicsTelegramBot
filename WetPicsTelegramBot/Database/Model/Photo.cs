using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }

        public string FromUserId { get; set; }

        public string ChatId { get; set; }

        public int MessageId { get; set; }
    }
}