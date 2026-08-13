// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2023/6/23
// // describe:
// //----------------------------------------------------------------*/
//
// using System.Linq;
// using EasyFramework;
// using UnityEditor;
// using UnityEngine;
//
// namespace EasyFramework.Editor
// {
//     public class TPAtlasSystemProvider : EasyFrameworkSettingsProvider<TPAtlasSystemProvider>
//     {
//         [SettingsProvider]
//         public static SettingsProvider Create() => GetOrCreate();
//         
//         public TPAtlasSystemProvider() : base(EasyFrameworkProvider.ToChildProvider<TPAtlasSystem>()) { }
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
//             foreach (var value in F.TPAtlasSystem.AtlasDict.Values)
//             {
//                 EditorGUILayout.LabelField($"{value.AtlasName}", $"IsLoaded: {value.IsLoaded}, NeverUnload: {value.NeverUnload}");
//             }
//         }
//     }
// }