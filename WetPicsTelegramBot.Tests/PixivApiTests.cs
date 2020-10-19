using System.Threading.Tasks;
using PixivApi;
using Xunit;

namespace WetPicsTelegramBot.Tests
{
    public partial class PixivApiTests
    {
        [Fact]
        public async Task TestAuth()
        {
            var auth = await Auth.AuthorizeAsync(
                Username,
                Pass,
                ClientId,
                ClientSecret);

            var latest = await auth.GetLatestWorksAsync();

            Assert.NotNull(latest);
        }
    }
}
