using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace WetPicsTelegramBot.Services.Dialog
{
    class TopRequestArgs
    {
        private static readonly Regex _periodRegex = new Regex(@"(period|p):(?<period>day|d|month|m|year|y)");
        private static readonly Regex _countRegex = new Regex(@"(count|c):(?<count>\d*)");

        private int _count = 5;

        public TopRequestArgs(string args)
        {
            var period = _periodRegex.Match(args).Groups["period"].Captures.FirstOrDefault();

            if (period != null)
            {
                SetTopPeriod(period.Value);
            }

            var count = _countRegex.Match(args).Groups["count"].Captures.FirstOrDefault();

            if (count != null)
            {
                Count = Int32.Parse(count.Value);
            }
        }

        public int Count
        {
            get => _count;
            set
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
            else
            {
                TopPeriod = TopPeriod.AllTime;
            }
        }
    }
}