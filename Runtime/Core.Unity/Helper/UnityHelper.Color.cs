/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    public static partial class UnityHelper
    {
        
        /// <summary>
        /// 转换成颜色值 格式为 255,255,255,255
        /// </summary>
        /// <param name="str">255,255,255,255</param>
        /// <param name="splitStr">间隔符</param>
        /// <returns></returns>
        public static Color SplitToColor(this string str, char splitStr = ',')
        {
            string[] arr = str.Split(splitStr);
            if (arr.Length != 4)
                return Color.white;
            return new Color(arr[0].ToFloat() / 255f, arr[1].ToFloat() / 255f, arr[2].ToFloat() / 255f, arr[3].ToFloat() / 255f);
        }
        
        /// <summary>
        /// 16进制颜色转换
        /// </summary>
        /// <param name="str"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static Color ToHexColor(this string str, float alpha = 1)
        {
            ColorUtility.TryParseHtmlString(str, out var col);
            col.a = alpha;
            return col;
        }
        
        public static bool TryToHexColor(this string str, out Color col, float alpha = 1)
        {
            bool result = ColorUtility.TryParseHtmlString(str, out col);
            col.a = alpha;
            return result;
        }

    }
}
