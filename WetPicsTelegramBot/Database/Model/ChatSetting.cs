using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Database.Model
{
    public class ChatSetting
    {
        [Key]
        public int Id { get; set; }

        public string ChatId { get; set; }

        public string TargetId { get; set; }
    }
}