using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Database.Model
{
    public class PixivImagePost
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int PixivIllustrationId { get; set; }


        [Required]
        public int PixivSettingId { get; set; }

        public PixivSetting PixivSetting { get; set; }
    }
}