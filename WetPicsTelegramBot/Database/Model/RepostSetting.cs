using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Database.Model
{
    public class RepostSetting : EntityBase
    {
        [Required]
        public long ChatId { get; set; }

        [Required]
        public string TargetId { get; set; }
    }
}