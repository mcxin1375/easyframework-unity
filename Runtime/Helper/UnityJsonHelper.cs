// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2018/2/23
// // describe:
// //----------------------------------------------------------------*/
//
// using System.IO;
// using UnityEngine;
//
// namespace EasyFramework
// {
//     public static class UnityJsonHelper
//     {
//         public static T LoadOrCreate<T>(string file) where T : new() => Load<T>(file) ?? new T();
//         public static T Load<T>(string file)
//         {
//             if (!File.Exists(file)) return default;
//             return LoadFromText<T>(File.ReadAllText(file));
//         }
//
//         public static T LoadOrCreateFromText<T>(string content) where T : new()
//         {
//             return LoadFromText<T>(content) ?? new T();
//         }
//         public static T LoadFromText<T>(string content)
//         {
//             if (string.IsNullOrEmpty(content)) return default;
//             
//             return JsonUtility.FromJson<T>(content);
//         }
//
//         public static void Save(string saveFile, object obj) => Save(saveFile, JsonUtility.ToJson(obj));
//         public static void Save(string saveFile, object obj, bool prettyPrint) => Save(saveFile, JsonUtility.ToJson(obj, prettyPrint));
//         public static void Save(string saveFile, string content)
//         {
//             if (string.IsNullOrEmpty(saveFile)) return;
//             string dirName = Path.GetDirectoryName(saveFile);
//             if (!string.IsNullOrEmpty(dirName) && !Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
//             File.WriteAllText(saveFile, content);
//         }
//     }
// }