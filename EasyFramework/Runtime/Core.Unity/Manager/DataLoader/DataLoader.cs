// using System.IO;
//
// namespace EasyFramework
// {
//     public class DataLoader : Singleton<DataLoader>, IDataLoader
//     {
//         public string GetDataFile(string fullName) => $"{EasyFrameworkSettings.DataPath}/{fullName}";
//         
//         public string LoadDataAllText(string fullName)
//         {
//             string file = GetDataFile(fullName);
//             if (!File.Exists(file))
//             {
//                 FDebug.LogError($"F.DataLoader.LoadDataAllText({fullName}) file not exists: {file}", LogTag.EasyFramework);
//                 return string.Empty;
//             }
//             return File.ReadAllText(file);
//         }
//         
//         public byte[] LoadDataAllBytes(string fullName)
//         {
//             string file = GetDataFile(fullName);
//             if (!File.Exists(file))
//             {
//                 FDebug.LogError($"F.DataLoader.LoadDataAllBytes({fullName}) file not exists: {file}", LogTag.EasyFramework);
//                 return null;
//             }
//             return File.ReadAllBytes(file);
//         }
//     }
// }