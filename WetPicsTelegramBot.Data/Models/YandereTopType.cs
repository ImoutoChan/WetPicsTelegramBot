using System.ComponentModel;

namespace WetPicsTelegramBot.Data.Models
{
    public enum YandereTopType
    {
        [Description("1d")]
        Day,

        [Description("1w")]
        Week,

        [Description("1m")]
        Month,

        // not supported
        //[Description("1y")]
        //Year
    }
}