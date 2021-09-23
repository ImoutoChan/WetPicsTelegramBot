using System.Threading.Tasks;

namespace PixivApi
{
    public interface IPixivAuthorization
    {
        Task<string> GetAccessToken();

        void ResetAccessToken();
    }
}
