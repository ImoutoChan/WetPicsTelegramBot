using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Data.Entities.ImageSources
{
    public class ImageSourcesChatSetting : EntityBase
    {
        [Required]
        public long ChatId { get; set; }

        [Required]
        public int MinutesInterval { get; set; }

        public DateTimeOffset? LastPostedTime { get; set; }


        public List<ImageSourceSetting> ImageSourceSettings { get; set; } 
            = new List<ImageSourceSetting>();

        public List<PostedImage> PostedImages { get; set; } 
            = new List<PostedImage>();
    }
}