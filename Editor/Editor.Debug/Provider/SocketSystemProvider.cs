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
//     public class SocketSystemProvider : EasyFrameworkSettingsProvider<SocketSystemProvider>
//     {
//         [SettingsProvider]
//         public static SettingsProvider Create() => GetOrCreate();
//         
//         public SocketSystemProvider() : base(EasyFrameworkProvider.ToChildProvider<SocketManager>()) { }
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
//             if (!SocketManager.HasInstance()) return;
//             
//             foreach (var kv in SocketManager.Instance.DebugList)
//             {
//                 EditorGUILayout.LabelField($"{kv.Host} : {kv.Port}", $"IsConnected: {kv.IsConnected}");
//             }
//         }
//     }
// }