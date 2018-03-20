using System.ComponentModel;

namespace WetPicsTelegramBot.Data.Models
{
    public enum DanbooruTopType
    {
        [Description("day")]
        Day,

        [Description("week")]
        Week,

        [Description("month")]
        Month
    }
}