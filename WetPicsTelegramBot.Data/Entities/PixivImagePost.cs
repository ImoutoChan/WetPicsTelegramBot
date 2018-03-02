using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Data.Entities
{
    public class PixivImagePost : EntityBase
    {
        [Required]
        public int PixivIllustrationId { get; set; }


        [Required]
        public int PixivSettingId { get; set; }

        public PixivSetting PixivSetting { get; set; }
    }
}