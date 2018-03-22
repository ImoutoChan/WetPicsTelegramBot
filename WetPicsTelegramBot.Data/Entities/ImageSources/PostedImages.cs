using System.ComponentModel.DataAnnotations;
using WetPicsTelegramBot.Data.Models;

namespace WetPicsTelegramBot.Data.Entities.ImageSources
{
    public class PostedImage : EntityBase
    {
        [Required]
        public ImageSource ImageSource { get; set; }
        
        public int PostId { get; set; }

        [Required]
        public int ImageSourcesChatSettingId { get; set; }


        public ImageSourcesChatSetting ImageSourcesChatSetting { get; set; }
    }
}