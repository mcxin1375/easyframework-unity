/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;

namespace EasyFramework
{
    public static partial class UnityHelper
    {

        /// <summary>
        /// 转换成Vector2。 字符串格式为（x,y）
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChar">分割符,默认为 ,</param>
        /// <returns></returns>
        public static Vector2 ToVector2(this string str, char splitChar = ',')
        {
            Vector2 result = Vector2.zero;
            if (!string.IsNullOrEmpty(str))
            {
                string[] arr = str.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length == 2)
                {
                    result = new Vector2(float.Parse(arr[0]), float.Parse(arr[1]));
                }
            }

            return result;
        }
    }
}
