using System.ComponentModel;

namespace WetPicsTelegramBot.Data.Models
{
    /// <summary>
    /// [ daily, weekly, monthly, male, female, rookie, daily_r18, weekly_r18, male_r18, female_r18, r18g ]
    /// </summary>
    public enum PixivTopType
    {
        [Description("daily")]
        DailyGeneral,

        [Description("daily_r18")]
        DailyR18,

        [Description("weekly")]
        WeeklyGeneral,

        [Description("weekly_r18")]
        WeeklyR18,

        [Description("monthly")]
        Monthly,

        [Description("rookie")]
        Rookie,

        [Description("original")]
        Original,

        [Description("male")]
        ByMaleGeneral,

        [Description("male_r18")]
        ByMaleR18,

        [Description("female")]
        ByFemaleGeneral,

        [Description("female_r18")]
        ByFemaleR18,

        [Description("r18g")]
        R18G
    }
}