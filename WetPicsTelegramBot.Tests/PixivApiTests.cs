using System.Threading.Tasks;
using PixivApi;
using PixivApi.Services;
using Xunit;

namespace WetPicsTelegramBot.Tests
{
    public partial class PixivApiTests
    {
        [Fact]
        public async Task TestAuth()
        {
            var tokenProvider = new PixivApiProvider();
            var api = await tokenProvider.GetApiAsync(
                Username,
                Pass,
                ClientId,
                ClientSecret);
            tokenProvider.ForceReAuth();
            api = await tokenProvider.GetApiAsync(
                Username,
                Pass,
                ClientId,
                ClientSecret);
            var latest = await api.GetLatestWorksAsync();

            Assert.NotNull(latest);
        }
    }
}
