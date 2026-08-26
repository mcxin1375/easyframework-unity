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
        /// 转换成Vector3。 字符串格式为（x,y,z）
        /// </summary>
        /// <param name="str">x,y,z</param>
        /// <param name="splitChar">分割符,默认为 ,</param>
        /// <returns></returns>
        public static Vector3 ToVector3(this string str, char splitChar = ',')
        {
            if (string.IsNullOrEmpty(str))
            {
                return Vector3.zero;
            }

            string[] array = str.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
            return array.Length == 3 ? new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2])) : Vector3.zero;
        }

    }
}
