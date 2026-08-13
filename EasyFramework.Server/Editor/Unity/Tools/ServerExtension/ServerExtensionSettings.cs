// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2023/6/23
// // describe:
// //----------------------------------------------------------------*/
//
// using EasyFramework.Editor;
// using EasyFramework.AOT;
//
// namespace EasyFramework.Server.Editor
// {
//     [EasyFramework.AOT.ProjectSettings("ProjectSettings/EasyFramework")]
//     public class ServerExtensionSettings : ProjectSettingsEditor<ServerExtensionSettings>
//     {
//         public ProjectUnityConfig projectUnityConfig;
//
//         protected override void OnCreate()
//         {
//             projectUnityConfig = new ProjectUnityConfig();
//             
//             var apps = ReflectionHelper.CreateInstances<IApp>();
//             foreach (var app in apps)
//             {
//                 AppConfig appConfig = new AppConfig();
//                 appConfig.appName = app.AppName;
//                 appConfig.defineSymbols = app.AppSymbols;
//                 projectUnityConfig.appList.Add(appConfig);
//             }
//
//             if (ResImporterSettings.Instance.resImporterConfigs?.Length > 0)
//                 projectUnityConfig.resImporterConfigList.AddRange(ResImporterSettings.Instance.resImporterConfigs);
//             if (DingTalkExtensionSettings.Instance.dingTalkConfigs?.Length > 0)
//                 projectUnityConfig.dingTalkConfigList.AddRange(DingTalkExtensionSettings.Instance.dingTalkConfigs);
//             projectUnityConfig.excelCommandSettings = ExcelImporterSettings.Instance.excelCommandSettings;
//             projectUnityConfig.protocCommandSettings = ProtocImporterSettings.Instance.protocCommandSettings;
//             
//         }
//     }
// }