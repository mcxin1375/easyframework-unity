// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2023/6/23
// // describe:
// //----------------------------------------------------------------*/
//
// using System.Linq;
// using UnityEditor;
// using UnityEngine;
//
// namespace EasyFramework.Editor
// {
//     public class ObjectEventSystemProvider : EasyFrameworkSettingsProvider<ObjectEventSystemProvider>
//     {
//         [SettingsProvider]
//         public static SettingsProvider Create() => GetOrCreate();
//         
//         public ObjectEventSystemProvider() : base(EasyFrameworkProvider.ToChildProvider<ObjectEventSystem>()) { }
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
//             foreach (var objectEventInfo in F.ObjectEventSystem.ObjectEventInfoDict.Values)
//             {
//                 EditorGUILayout.HelpBox($"ObjectType: {objectEventInfo.ObjectType.Name}", MessageType.Info);
//                 
//                 var keys = objectEventInfo.EventDict.Keys.OrderBy(i => i.Name).ToArray();
//                 foreach (var key in keys)
//                 {
//                     var interfaceType = key;
//                     var objectList = objectEventInfo.EventDict[key];
//                     
//                     EditorGUILayout.LabelField($"{ProviderGUIStyle.MainPrefix}{interfaceType.Name}", ProviderGUIStyle.MainStyle);
//                     for (int i = 0; i < objectList.Count; i++)
//                     {
//                         var ex = objectList[i];
//                         EditorGUILayout.LabelField($"{ProviderGUIStyle.DependencyPrefix}{ex.order}. {ex.instance.GetType().Name}", ProviderGUIStyle.DependencyStyle);
//                     }
//                 }
//                 EditorGUILayout.Space(5);
//             }
//             
//         }
//     }
// }