/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public static class StringHelper
    {
        public static float ToFloat(this string str)
        {
            if (float.TryParse(str, out var result)) return result;
            return 0;
        }

        public static int ToInt(this string str)
        {
            if (int.TryParse(str, out var result)) return result;
            return 0;
        }
        
        public static bool IsMatch(this string str, string start, string end)
        {
            if (str == null) return false;
            if (start != null && !str.StartsWith(start, StringComparison.Ordinal)) return false;
            if (end != null && !str.EndsWith(end, StringComparison.Ordinal)) return false;
            return true;
        }
        
        public static string Extract(this string str, string start, string end)
        {
            int startIndex = str.IndexOf(start, StringComparison.Ordinal);
            if (startIndex < 0) return string.Empty;
            startIndex += start.Length;
            int endIndex = str.IndexOf(end, startIndex, StringComparison.Ordinal);
            if (endIndex < 0) return string.Empty;
            return str.Substring(startIndex, endIndex - startIndex);
        }

        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);
        public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

        // public static string SubString(this string str, string start, string end)
        // {
        //     string pattern = $"{Regex.Escape(start)}(.*?){Regex.Escape(end)}";
        //     Match match = Regex.Match(str, pattern);
        //     return match.Success && match.Groups.Count > 0 ? match.Groups[1].Value : string.Empty;
        // }
    }
}