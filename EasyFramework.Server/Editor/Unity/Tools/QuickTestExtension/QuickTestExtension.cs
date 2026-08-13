// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2023/6/23
// // describe:
// //----------------------------------------------------------------*/
//
// using EasyFramework.Editor;
//
// namespace EasyFramework.Server.Editor
// {
//     public class QuickTestExtension
//     {
//         public static void Execute()
//         {
//             var settings = QuickTestExtensionSettings.Instance;
//
//             if (settings.generateAll)
//                 HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
//             else if(settings.compileDllActiveBuildTarget)
//                 HybridCLR.Editor.Commands.CompileDllCommand.CompileDllActiveBuildTarget();
//
//             if (settings.assetBundleBuilder) AssetBundleBuilder.Instance.BuildAssetBundle();
//             if (settings.dllBuilder) HybridCLRBuilder.Instance.Build();
//             if (settings.dataBuilder) DataBuilder.Instance.Build();
//             if (settings.dlcBuilder) DLCBuilder.Instance.Build();
//
//             if (settings.uploadDLCApp) ServerEditorAPI.UploadDLCAppAsync();
//         }
//     }
// }