using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WetPicsTelegramBot.Data.Models;

namespace WetPicsTelegramBot.Data.Entities
{
    public class ImageSourceChatSetting : EntityBase
    {
        [Required]
        public long ChatId { get; set; }

        [Required]
        public int MinutesInterval { get; set; }

        public DateTimeOffset? LastPostedTime { get; set; }
    }

    public class PixivImagePost : EntityBase
    {
        [Required]
        public int PixivIllustrationId { get; set; }


        [Required]
        public int PixivSettingId { get; set; }

        public PixivSetting PixivSetting { get; set; }
    }

    public class ImageSourceSettings : EntityBase
    {
        [Required]
        public ImageSource ImageSource { get; set; }

        public string Options { get; set; }

        public int ChatSettingId { get; set; }


        public ImageSourceChatSetting ChatSetting { get; set; }

        public List<PixivImagePost> PixivImagePosts { get; set; }
            = new List<PixivImagePost>();
    }
}