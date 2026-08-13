// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2023/6/23
// // describe:
// //----------------------------------------------------------------*/
//
// using System.Threading.Tasks;
// using EasyFramework.Editor;
// using UnityEditor;
// using UnityEngine;
//
// namespace EasyFramework.Server.Editor
// {
//     public class SVCErrorLogCollectorProvider : ProjectSettingsProvider<SVCErrorLogCollectorProvider>
//     {
//         private SVCErrorConfig _config;
//         
//         [SettingsProvider]
//         public static SettingsProvider Create() => GetOrCreate();
//         
//         public SVCErrorLogCollectorProvider() : base(EasyFrameworkServerProvider.ToChildProvider(nameof(SVCErrorLogCollectorSettings))) { }
//
//         protected override ScriptableObject[] LoadObjects()
//         {
//             _ = RefreshConfigAsync();
//             return new ScriptableObject[]
//             {
//                 SVCErrorLogCollectorSettings.CreateInstance(),
//             };
//         }
//         
//         private async Task RefreshConfigAsync()
//         {
//             // Log.Info(ServerURL.GetSVCErrorConfig);
//             _config = await ServerEditorAPI.GetSVCErrorConfigAsync();
//         }
//         
//         protected override void OnAfterDraw()
//         {
//             if (_config != null)
//             {
//                 foreach (var log in _config.ErrorLogs)
//                 {
//                     EditorGUILayout.LabelField(log, EditorStyles.wordWrappedLabel);
//                 }
//             }
//         }
//     }
// }