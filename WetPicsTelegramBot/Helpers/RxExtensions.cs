using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace WetPicsTelegramBot.Helpers
{
    static class RxExtensions
    {
        public static IObservable<object> HandleAsync<TSource>(this IObservable<TSource> source, Func<TSource, Task> selector)
        {
            return source.SelectMany(async item =>
            {
                await selector(item);
                return (object)null;
            });
        }
    }
}
