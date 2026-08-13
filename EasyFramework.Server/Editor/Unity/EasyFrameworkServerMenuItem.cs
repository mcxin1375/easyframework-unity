// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2024/5/8
// // describe:
// //----------------------------------------------------------------*/
//
// using EasyFramework.Editor;
// using UnityEditor;
//
// namespace EasyFramework.Server.Editor
// {
//     public static class EasyFrameworkToolsExMenuItem
//     {
//         private const int Priority = EasyFrameworkEditorMenuItemOrder.Server;
//         
//         // [MenuItem("EasyFramework/Server/SVCErrorLogExtension - UpdateFromServer", priority = Priority + 100)]
//         // private static void SVCErrorLogExtension_UpdateFromServer() => _ = SVCErrorLogExtension.UpdateFromServerAsync();
//         // [MenuItem("EasyFramework/Server/SVCErrorLogExtension - ClearServerData", priority = Priority + 100)]
//         // private static void SVCErrorLogExtension_ClearSVCErrorConfig() => SVCErrorLogExtension.ClearSVCErrorConfig();
//         
//         [MenuItem("EasyFramework/Server/ServerEditorAPI - UploadProjectConfig", priority = Priority + 200)]
//         private static void ServerExtension_UploadProjectConfig() => _ = ServerEditorAPI.UploadProjectConfigAsync();
//         
//         [MenuItem("EasyFramework/Server/ServerEditorAPI - UploadDLCApp", priority = Priority + 200)]
//         private static void ServerExtension_UploadDLCApp() => ServerEditorAPI.UploadDLCAppAsync();
//         
//         // [MenuItem("EasyFramework/Server/QuickTestExtension - Execute", priority = Priority + 400)]
//         // private static void QuickTestExtension_Execute() => QuickTestExtension.Execute();
//     }
// }