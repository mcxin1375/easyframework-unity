// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2023/6/23
// // describe:
// //----------------------------------------------------------------*/
//
// using EasyFramework.Editor;
// using UnityEditor;
// using UnityEngine;
//
// namespace EasyFramework.Server.Editor
// {
//     public class ServerExtensionProvider : ProjectSettingsProvider<ServerExtensionProvider>
//     {
//         
//         [SettingsProvider]
//         public static SettingsProvider Create() => GetOrCreate();
//         
//         public ServerExtensionProvider() : base(EasyFrameworkServerProvider.ToChildProvider(nameof(ServerExtensionSettings))) { }
//
//         protected override ScriptableObject[] LoadObjects()
//         {
//             return new ScriptableObject[]
//             {
//                 ServerExtensionSettings.CreateInstance(),
//             };
//         }
//
//         protected override void OnAfterDrawSettings(string settingsName)
//         {
//             base.OnAfterDrawSettings(settingsName);
//             
//             if (GUILayout.Button("ServerConfig Sync",  GUILayout.Width(150),  GUILayout.Height(30)))
//             {
//                 _ = ServerEditorAPI.UploadProjectConfigAsync();;
//             }
//         }
//     }
// }