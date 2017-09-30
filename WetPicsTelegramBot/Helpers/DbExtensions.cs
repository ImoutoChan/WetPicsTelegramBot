using System;
using System.Linq;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Helpers
{
    static class DbExtensions
    {
        public static IQueryable<T> FilterByDates<T>(this IQueryable<T> query, DateTimeOffset? from, DateTimeOffset? to)
            where T : EntityBase
        {
            if (from != null)
            {
                query = query.Where(x => x.AddedDate >= from);
            }
            if (to != null)
            {
                query = query.Where(x => x.AddedDate <= to);
            }

            return query;
        }
    }
}
