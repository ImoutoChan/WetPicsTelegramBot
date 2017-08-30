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

        public static IObservable<TSource> HandleException<TSource, TException>(this IObservable<TSource> source,
                                                                               Action<TException> action,
                                                                               bool rethrow = false)
            where TException : Exception
        {
            return source.Catch<TSource, TException>(ex =>
            {
                action(ex);

                if (rethrow)
                    throw ex;

                return Observable.Empty<TSource>();
            });
        }
    }
}
