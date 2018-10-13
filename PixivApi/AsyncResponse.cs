using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PixivApi
{
    public class AsyncResponse : IDisposable
    {
        public AsyncResponse(HttpResponseMessage source)
        {
            Source = source;
        }

        private HttpResponseMessage Source { get; }

        public Task<Stream> GetResponseStreamAsync()
        {
            return Source.Content.ReadAsStreamAsync();
        }

        public Task<string> GetResponseStringAsync()
        {
            return Source.Content.ReadAsStringAsync();
        }

        public Task<byte[]> GetResponseByteArrayAsync()
        {
            return Source.Content.ReadAsByteArrayAsync();
        }

        public void Dispose()
        {
            Source?.Dispose();
        }
    }
}
