namespace WetPicsTelegramBot.Database
{
    internal class Stats
    {
        public Stats(int picCount, int getLikeCount, int setLikeCount, int setSelfLikeCount)
        {
            PicCount = picCount;
            GetLikeCount = getLikeCount;
            SetLikeCount = setLikeCount;
            SetSelfLikeCount = setSelfLikeCount;
        }

        public int PicCount { get; }

        public int GetLikeCount { get; }

        public int SetLikeCount { get; }

        public int SetSelfLikeCount { get; }
    }
}