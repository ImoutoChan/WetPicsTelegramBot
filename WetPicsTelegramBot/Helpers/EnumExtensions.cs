using System;
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
    }
}
