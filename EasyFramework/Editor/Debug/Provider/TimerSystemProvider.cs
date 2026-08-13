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
//     public class TimerSystemProvider : EasyFrameworkSettingsProvider<TimerSystemProvider>
//     {
//         private bool _stackTraceActive;
//         
//         [SettingsProvider]
//         public static SettingsProvider Create() => GetOrCreate();
//         
//         public TimerSystemProvider() : base(EasyFrameworkProvider.ToChildProvider<TimerSystem>()) { }
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
//             _stackTraceActive = EditorGUILayout.Toggle("StackTraceActive", _stackTraceActive);
//             EditorGUILayout.LabelField("Time", $"{EasyTask.Time}");
//             foreach (var kv in EasyTask.Timer.TimerDict)
//             {
//                 var key = kv.Value.TimerObject == null ? "-" : kv.Value.TimerObject.ToString();
//                 EditorGUILayout.LabelField($"Token: {kv.Key}, Interval: {kv.Value.Interval}, TimerObject: {key}");
//
//                 if (_stackTraceActive)
//                 {
//                     if (!EasyTask.Timer.DebugDict.TryGetValue(kv.Key, out var stackTrace)) continue;
//                     EditorGUILayout.LabelField($"{stackTrace}", EditorStyles.wordWrappedLabel);
//                 }
//             }
//
//         }
//     }
// }