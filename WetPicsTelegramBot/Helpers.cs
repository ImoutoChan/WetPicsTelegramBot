using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot
{
    public static class HelpersExtensions
    {
        public static string GetBeautyName(this User user)
        {
            var userSb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(user.FirstName))
            {
                userSb.Append(user.FirstName + " ");
            }
            if (!String.IsNullOrWhiteSpace(user.LastName))
            {
                userSb.Append(user.LastName + " ");
            }
            if (!String.IsNullOrWhiteSpace(user.Username))
            {
                userSb.Append(userSb.Length == 0
                    ? $"@{user.Username}" :
                    $"(@{user.Username})");
            }
            if (userSb.Length == 0)
            {
                userSb.Append(user.Id);
            }

            var userName = userSb.ToString().Trim();
            return userName;
        }

        public static long ToLong(this ChatId chatId)
        {
            var chatIdLong = Int64.Parse(chatId);
            return chatIdLong;
        }

        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(new Random());
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            return source.ShuffleIterator(rng);
        }

        private static IEnumerable<T> ShuffleIterator<T>(
            this IEnumerable<T> source, Random rng)
        {
            var buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }
}