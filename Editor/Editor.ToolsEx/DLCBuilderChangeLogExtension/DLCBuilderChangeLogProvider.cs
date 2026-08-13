// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2023/6/23
// // describe:
// //----------------------------------------------------------------*/
//
// using System.IO;
// using UnityEditor;
// using UnityEngine;
//
// namespace EasyFramework.Editor
// {
//     public class DLCBuilderChangeLogProvider : EasyFrameworkSettingsProvider<DLCBuilderChangeLogProvider>
//     {
//         private enum ELogLocation
//         {
//             DLCBuilder,
//             DLCBuilderLan
//         }
//
//         private string _logText;
//         private Vector2 _scrollPosition; // 滚动条位置
//         private ELogLocation _logLocation = ELogLocation.DLCBuilderLan;
//
//         [SettingsProvider]
//         public static SettingsProvider Create() => GetOrCreate();
//
//         public DLCBuilderChangeLogProvider() : base(EasyFrameworkToolsExtensionProvider.ToChildProvider(". DLCBuilderChangeLog"))
//         {
//         }
//
//         protected override ScriptableObject[] LoadObjects()
//         {
//             var outputPath = _logLocation == ELogLocation.DLCBuilder
//                 ? $"{FEditor.DLCBuilder.ProjectDataSpace}/{DLCBuilderChangeLogExtension.LogFileName}"
//                 : $"{FEditor.DLCBuilder.ProjectDataSpaceLan}/{DLCBuilderChangeLogExtension.LogFileName}";
//             _logText = File.Exists(outputPath) ? File.ReadAllText(outputPath) : string.Empty;
//             
//             return new ScriptableObject[]
//             {
//                 // DLCBuilderUpdateLogSettings.Load(),
//             };
//         }
//
//         protected override void OnAfterDraw()
//         {
//             GUILayout.Space(10);
//             
//             EditorGUILayout.HelpBox(DLCBuilderChangeLogExtension.LogFileName, MessageType.Info);
//             var val = (ELogLocation)EditorGUILayout.EnumPopup("ELogLocation", _logLocation);
//             if (val != _logLocation)
//             {
//                 _logLocation = val;
//                 
//                 var outputPath = _logLocation == ELogLocation.DLCBuilder
//                     ? $"{FEditor.DLCBuilder.ProjectDataSpace}/{DLCBuilderChangeLogExtension.LogFileName}"
//                     : $"{FEditor.DLCBuilder.ProjectDataSpaceLan}/{DLCBuilderChangeLogExtension.LogFileName}";
//                 _logText = File.Exists(outputPath) ? File.ReadAllText(outputPath) : string.Empty;
//             }
//
//             _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
//             EditorGUILayout.LabelField(_logText, EditorStyles.wordWrappedLabel);
//             EditorGUILayout.EndScrollView();
//         }
//     }
// }