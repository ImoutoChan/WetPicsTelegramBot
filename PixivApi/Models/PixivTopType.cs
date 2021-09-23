using System.ComponentModel;

namespace PixivApi.Models
{
    public enum PixivTopType
    {
        [Description("day")]
        DailyGeneral,

        [Description("day_r18")]
        DailyR18,

        [Description("week")]
        WeeklyGeneral,

        [Description("week_r18")]
        WeeklyR18,

        [Description("month")]
        Monthly,

        [Description("week_rookie")]
        Rookie,

        [Description("week_original")]
        Original,

        [Description("day_male")]
        ByMaleGeneral,

        [Description("day_male_r18")]
        ByMaleR18,

        [Description("day_female")]
        ByFemaleGeneral,

        [Description("day_female_r18")]
        ByFemaleR18,

        [Description("week_r18g")]
        R18G
    }
}
