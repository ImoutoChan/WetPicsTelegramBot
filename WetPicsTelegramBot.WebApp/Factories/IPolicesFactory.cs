using System.Runtime.CompilerServices;
using Polly;

namespace WetPicsTelegramBot.WebApp.Factories
{
    public interface IPolicesFactory
    {
        IAsyncPolicy GetDefaultHttpRetryPolicy([CallerMemberName] string methodName = null);
    }
}