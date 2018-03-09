﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WetPicsTelegramBot.Data.Models;

namespace WetPicsTelegramBot.Data.Entities
{
    public class PixivSetting : EntityBase
    {
        [Required]
        public long ChatId { get; set; }

        [Required]
        public PixivTopType PixivTopType { get; set; }

        [Required]
        public int MinutesInterval { get; set; }

        public DateTimeOffset? LastPostedTime { get; set; }

        public List<PixivImagePost> PixivImagePosts { get; set; } = new List<PixivImagePost>();

        [NotMapped]
        public HashSet<int> PixivImagePostsSet { get; set; }
    }
}