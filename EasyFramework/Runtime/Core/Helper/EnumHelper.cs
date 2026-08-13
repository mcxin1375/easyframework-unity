// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2018/3/1
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
//
// namespace EasyFramework.Core
// {
//     public static class EnumHelper
//     {
//         public static T ParseEnum<T>(string str) => (T)Enum.Parse(typeof(T), str);
//         public static int GetEnumLength<T>() => Enum.GetValues(typeof(T)).Length;
//         public static string[] GetEnumNames<T>() => Enum.GetNames(typeof(T));
//     }
// }