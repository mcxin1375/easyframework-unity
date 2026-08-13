/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/3/18
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public enum FormatLevel
    {
        K1000 = 0,
        M1000000 = 1,
        B1000000000 = 2,
    }

    public static class FormatHelper
    {
        public const int Kb = 1024;
        public const int Mb = 1048576;
        public const int Gb = 1073741824;

        public const int K1000 = 1000;
        public const int M1000000 = 1000000;
        public const int B1000000000 = 1000000000;

        public static string FormatNumber(this int num, FormatLevel formatLevel = FormatLevel.K1000) => FormatNumber((float)num, formatLevel);
        public static string FormatNumber(this float num, FormatLevel formatLevel = FormatLevel.K1000)
        {
            if (formatLevel <= FormatLevel.B1000000000 && num >= B1000000000) return $"{num / B1000000000:N1}B";
            if (formatLevel <= FormatLevel.M1000000 && num >= M1000000) return $"{num / M1000000:N1}M";
            if (formatLevel <= FormatLevel.K1000 && num >= K1000) return $"{num / K1000:N1}K";
            return $"{num}";
        }
        public static string FormatNumber(this double num, FormatLevel formatLevel = FormatLevel.K1000)
        {
            if (formatLevel <= FormatLevel.B1000000000 && num >= B1000000000) return $"{num / B1000000000:N1}B";
            if (formatLevel <= FormatLevel.M1000000 && num >= M1000000) return $"{num / M1000000:N1}M";
            if (formatLevel <= FormatLevel.K1000 && num >= K1000) return $"{num / K1000:N1}K";
            return $"{num}";
        }

        public static string FormatByte(this int num, FormatLevel formatLevel = FormatLevel.K1000) => FormatByte((float)num, formatLevel);
        public static string FormatByte(this float num, FormatLevel formatLevel = FormatLevel.K1000)
        {
            if (formatLevel <= FormatLevel.B1000000000 && num >= Gb) return $"{num / Gb:N1}GB";
            if (formatLevel <= FormatLevel.M1000000 && num >= Mb) return $"{num / Mb:N1}MB";
            if (formatLevel <= FormatLevel.K1000 && num >= Kb) return $"{num / Kb:N1}KB";
            return $"{num}B";
        }
        public static string FormatByte(this double num, FormatLevel formatLevel = FormatLevel.K1000)
        {
            if (formatLevel <= FormatLevel.B1000000000 && num >= Gb) return $"{num / Gb:N1}GB";
            if (formatLevel <= FormatLevel.M1000000 && num >= Mb) return $"{num / Mb:N1}MB";
            if (formatLevel <= FormatLevel.K1000 && num >= Kb) return $"{num / Kb:N1}KB";
            return $"{num}B";
        }
    }
}