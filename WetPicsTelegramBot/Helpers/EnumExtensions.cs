using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace WetPicsTelegramBot.Helpers
{
    static class EnumExtensions
    {
        public static bool IsDefined<T>(this T value) where T : struct, IConvertible
        {
            if (!typeof(T).GetTypeInfo().IsEnum)
            {
                throw new NotSupportedException("T must be an enumerated type");
            }

            return Enum.IsDefined(typeof(T), value);
        }

        public static string GetEnumDescription(this Enum value)
        {
            var attributes = value
                .GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .OfType<DescriptionAttribute>()
                .ToArray();

            if (attributes?.Any() ?? false)
            {
                return attributes.First().Description;
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
