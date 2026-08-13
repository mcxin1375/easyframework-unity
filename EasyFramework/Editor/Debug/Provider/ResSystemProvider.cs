// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2023/6/23
// // describe:
// //----------------------------------------------------------------*/
//
// using UnityEditor;
// using UnityEngine;
//
// namespace EasyFramework.Editor
// {
//     public class ResSystemProvider : EasyFrameworkSettingsProvider<ResSystemProvider>
//     {
//         [SettingsProvider]
//         public static SettingsProvider Create() => GetOrCreate();
//         
//         public ResSystemProvider() : base(EasyFrameworkProvider.ToChildProvider<ResSystem>()) { }
//
//         protected override ScriptableObject[] LoadObjects()
//         {
//             return new ScriptableObject[]
//             {
//                 // settings,
//             };
//         }
//
//         protected override void OnAfterDraw()
//         {
//             base.OnAfterDraw();
//
//             if (!F.Initialized || !Application.isPlaying) return;
//
//             EditorGUILayout.HelpBox($"{nameof(ResPoolBehaviour)}", MessageType.Info);
//             foreach (var kv in F.ResLoader.ResPools)
//             {
//                 EditorGUILayout.LabelField($"{kv.ResName}", $"CreatedCount: {kv.CreatedCount}, PooledCount: {kv.PooledCount}");
//             }
//         }
//         
//     }
// }