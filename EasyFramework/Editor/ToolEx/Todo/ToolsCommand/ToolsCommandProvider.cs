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
//     public class ToolsCommandProvider : ProjectSettingsProvider<ToolsCommandProvider>
//     {
//         [SettingsProvider]
//         public static SettingsProvider Create() => GetOrCreate();
//         
//         public ToolsCommandProvider() : base(EasyFrameworkEditorProvider.ToChildProvider(nameof(ToolsCommand))) { }
//
//         protected override ScriptableObject[] LoadObjects()
//         {
//             return new ScriptableObject[]
//             {
//                 ToolsCommandSettings.CreateInstance(),
//             };
//         }
//     }
// }