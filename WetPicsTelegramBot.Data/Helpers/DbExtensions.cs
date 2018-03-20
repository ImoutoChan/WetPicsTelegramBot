using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using WetPicsTelegramBot.Data.Entities;

namespace WetPicsTelegramBot.Data.Helpers
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

        public static void AddPostgreSqlRules(this ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.Relational().TableName = entity.Relational().TableName.ToSnakeCase();
       
                foreach (var property in entity.GetProperties())
                {
                    property.Relational().ColumnName = property.Name.ToSnakeCase();
                }

                foreach (var key in entity.GetKeys())
                {
                    key.Relational().Name = key.Relational().Name.ToSnakeCase();
                }

                foreach (var key in entity.GetForeignKeys())
                {
                    key.Relational().Name = key.Relational().Name.ToSnakeCase();
                }

                foreach (var index in entity.GetIndexes())
                {
                    index.Relational().Name = index.Relational().Name.ToSnakeCase();
                }
            }
        }

        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
    }
}
