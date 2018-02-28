using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WetPicsTelegramBot.Helpers
{
    static class RxExtensions
    {
        public static IObservable<object> HandleAsyncWithLogging<TSource>(this IObservable<TSource> source, Func<TSource, Task> selector, ILogger logger)
        {
            return source.SelectMany(async item =>
            {
                try
                {
                    await selector(item);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error in handling observables.");
                }
                return (object)null;
            });
        }
    }
}
