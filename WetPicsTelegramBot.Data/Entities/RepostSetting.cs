using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Data.Entities
{
    public class RepostSetting : EntityBase
    {
        [Required]
        public long ChatId { get; set; }

        [Required]
        public string TargetId { get; set; }
    }
}