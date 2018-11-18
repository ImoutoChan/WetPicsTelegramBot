using System.Linq;
using System.Text.RegularExpressions;

namespace WetPicsTelegramBot.WebApp.Models
{
    class TopRequestArgs
    {
        private static readonly Regex _periodRegex = new Regex(@"(period|p):(?<period>day|d|month|m|year|y|week|w)");
        private static readonly Regex _countRegex = new Regex(@"(count|c):(?<count>\d*)");
        private static readonly Regex _albumRegex = new Regex(@"(album)");


        private int _count = 5;

        public TopRequestArgs(string args)
        {
            var period = _periodRegex.Match(args).Groups["period"].Captures.FirstOrDefault();
            if (period != null)
                SetTopPeriod(period.Value);

            var count = _countRegex.Match(args).Groups["count"].Captures.FirstOrDefault();
            if (count != null)
                Count = int.Parse(count.Value);

            var withAlbum = _albumRegex.Match(args).Captures.Any();

            WithAlbum = withAlbum;
        }

        public int Count
        {
            get => _count;
            private set
            {
                if (value > 20)
                {
                    value = 20;
                }
                if (value < 1)
                {
                    value = 1;
                }
                _count = value;
            }
        }

        public TopPeriod TopPeriod { get; private set; } = TopPeriod.AllTime;

        public bool WithAlbum { get; }

        private void SetTopPeriod(string period)
        {
            period = period.ToLower();

            if (new[] { "day", "d" }.Contains(period))
            {
                TopPeriod = TopPeriod.Day;
            }
            else if (new[] { "month", "m" }.Contains(period))
            {
                TopPeriod = TopPeriod.Month;
            }
            else if (new[] { "year", "y" }.Contains(period))
            {
                TopPeriod = TopPeriod.Year;
            }
            else if (new[] { "week", "w" }.Contains(period))
            {
                TopPeriod = TopPeriod.Week;
            }
            else
            {
                TopPeriod = TopPeriod.AllTime;
            }
        }
    }
}