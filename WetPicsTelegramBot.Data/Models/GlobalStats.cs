namespace WetPicsTelegramBot.Data.Models
{
    public class GlobalStats
    {
        public GlobalStats(int picCount, int likesCount, int picAnyLiked)
        {
            PicCount = picCount;
            LikesCount = likesCount;
            PicAnyLiked = picAnyLiked;
        }

        public int PicCount { get; }
        public int LikesCount { get; }
        public int PicAnyLiked { get; }
    }
}