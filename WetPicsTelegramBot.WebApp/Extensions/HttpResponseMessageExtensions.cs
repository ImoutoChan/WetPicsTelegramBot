using System.Linq;
using System.Net.Http;

namespace WetPicsTelegramBot.WebApp.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static bool TryGetLength(this HttpResponseMessage response, out int length)
        {
            length = default;

            if (!response.Content.Headers.TryGetValues("Content-Length", out var lengthStrings))
                return false;

            var lengthParams = lengthStrings.ToList();

            return lengthParams.Any() && int.TryParse(lengthParams.First(), out length);
        }
    }
}
