using System;
using Microsoft.EntityFrameworkCore;

namespace WetPicsTelegramBot.Database
{
    internal abstract class Repository<T> where T : DbContext
    {
        protected Repository(IServiceProvider serviceProvider)
        {
            GetDbContext = () => (T)serviceProvider.GetService(typeof(T));
        }

        protected Func<T> GetDbContext { get; }
    }
}