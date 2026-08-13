// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2024/5/8
// // describe:
// //----------------------------------------------------------------*/
//
// using UnityEditor;
// using UnityEngine;
//
// namespace EasyFramework.Editor
// {
//     public static class EasyFrameworkWindowMenuItem
//     {
//         private const int Priority = EasyFrameworkEditorMenuItemOrder.Window;
//         
//         [MenuItem("EasyFramework/Window/ExcelViewerWindow", priority = Priority + 100)]
//         private static void ExcelViewerWindow_Open() => ExcelViewerWindow.Open();
//         [MenuItem("EasyFramework/Window/ResDebugWindow", priority = Priority + 200)]
//         private static void ResDebugWindow_Open() => ResDebugWindow.Open(new Vector2(500, 300));
//         
//         // [MenuItem("EasyFramework/Debug/Debug Tools/SceneBatchingDebug", priority = Priority + 800)]
//         // static void SceneBatchingDebug()
//         // {
//         //     DebugTool.SceneBatchingDebug();
//         // }
//     }
// }