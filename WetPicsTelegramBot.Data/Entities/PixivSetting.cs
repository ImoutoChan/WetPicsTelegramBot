using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WetPicsTelegramBot.Data.Models;

namespace WetPicsTelegramBot.Data.Entities
{
    public abstract class ImageSourceSetting : EntityBase
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

    public class PixivSetting : ImageSourceSetting
    {
        [Required]
        public PixivTopType PixivTopType { get; set; }
    }

    public class DanbooruSetting : ImageSourceSetting
    {
        [Required]
        public DanbooruTopType DanbooruTopType { get; set; }
    }

    public class ImageSourceSettings : EntityBase
    {
        [Required]
        public long ChatId { get; set; }

        [Required]
        public ImageSource ImageSource { get; set; }

        [Required]
        public int MinutesInterval { get; set; }

        public DateTimeOffset? LastPostedTime { get; set; }

        public string Options { get; set; }

        public List<PixivImagePost> PixivImagePosts { get; set; }
            = new List<PixivImagePost>();
    }
}