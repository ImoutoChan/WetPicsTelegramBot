using System.ComponentModel.DataAnnotations;
using WetPicsTelegramBot.Data.Models;

namespace WetPicsTelegramBot.Data.Entities.ImageSources
{
    public class ImageSourceSetting : EntityBase
    {
        [Required]
        public ImageSource ImageSource { get; set; }

        public string Options { get; set; }

        [Required]
        public int ImageSourcesChatSettingId { get; set; }


        public ImageSourcesChatSetting ImageSourcesChatSetting { get; set; }
    }
}