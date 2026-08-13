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
//     public class QuickTestExtensionProvider : EasyFrameworkSettingsProvider<QuickTestExtensionProvider>
//     {
//         
//         [SettingsProvider]
//         public static SettingsProvider Create() => GetOrCreate();
//         
//         public QuickTestExtensionProvider() : base(EasyFrameworkServerProvider.ToChildProvider(nameof(QuickTestExtensionSettings))) { }
//
//         protected override ScriptableObject[] LoadObjects()
//         {
//             return new ScriptableObject[]
//             {
//                 QuickTestExtensionSettings.CreateInstance(),
//             };
//         }
//
//         protected override void OnAfterDrawSettings(string settingsName)
//         {
//             base.OnAfterDrawSettings(settingsName);
//             
//             if (GUILayout.Button("Execute",  GUILayout.Width(150),  GUILayout.Height(30)))
//             {
//                 QuickTestExtension.Execute();
//             }
//         }
//     }
// }