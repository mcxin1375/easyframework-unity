/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class ToolsCommand : EditorTool<ToolsCommand>
    {
        [MenuItem("EasyFramework/Tools/Tools Command - BuildSettings", priority = EasyFrameworkToolsSettings.ToolsCommand)]
        public static void MenuItem() => ToolsCommand.Instance.BuildSettings();
        
        public void BuildSettings()
        {
            var settings = ToolsCommandSettings.Instance;
            
            // EasyFrameworkAOTSettings.Instance.dlcEnabled = settings.hotUpdate;
            // EasyFrameworkAOTSettings.Instance.SaveEx();
            // AssetDatabase.SaveAssets();
            
            PlayerBuilderSettings.Instance.dlcVersion = settings.dlcVersion;
            PlayerBuilderSettings.Instance.developmentBuild = settings.developmentBuild;
            PlayerBuilderSettings.Instance.buildPlayer = settings.buildPlayer;
            PlayerBuilderSettings.Instance.buildProject = settings.buildProject;
            PlayerBuilderSettings.Instance.SaveEx();
            
            // if (settings.dllBuilder || settings.buildPlayer || settings.buildProject)
            // {
            //     HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
            // }

            if (settings.assetImporter) AssetImporter.Instance.Execute();
            if (settings.assetCreator) AssetCreator.Instance.Execute();
            
            if (settings.assetBundleBuilder) AssetBundleBuilder.Instance.Build();
            if (settings.dllBuilder) HybridCLRBuilder.Instance.Build();
            if (settings.dataBuilder) DataBuilder.Instance.Build();
            if (settings.dlcBuilder) DLCBuilder.Instance.Build();
            if (settings.dlcReleaseBuilder) DLCReleaseBuilder.ReleaseCurrent();
            
            if (settings.buildPlayer || settings.buildProject) PlayerBuilder.Instance.BuildSettings();
            
        }
        
        public static void BatchMode()
        {
            Debug.Log("ToolsCommand.BatchMode");
            // EasyFrameworkEditorSettings.BatchMode = true;
            
            string[] cmdArgs = System.Environment.GetCommandLineArgs();
            var commandParser = new CommandLineParams(cmdArgs);

            var dataSpace = commandParser.GetFirstOrEmpty("-dataSpace");
            Debug.Log($"-dataSpace: {dataSpace}");
            if (!string.IsNullOrWhiteSpace(dataSpace))
            {
                EasyFrameworkPreferences.ProjectDataPath = dataSpace;
            }

            var taskConfigFile = commandParser.GetFirstOrEmpty("-taskConfigFile");
            Debug.Log($"-taskConfigFile: {taskConfigFile}");
            var taskConfig = TaskConfig.LoadFromFile(taskConfigFile);
            
            Debug.Log($"AppName: {EasyFrameworkAOTSettings.App.AppName}");
            Debug.Log($"DataSpace: {EasyFrameworkPreferences.ProjectDataPath}");
            
            Debug.Log(taskConfig.ToJsonEx(true));
            
            var settings = ToolsCommandSettings.Instance;
            settings.dlcVersion = taskConfig.DLCVersion;
            settings.hotUpdate = taskConfig.HotUpdate;
            settings.developmentBuild = taskConfig.DevelopmentBuild;
            
            settings.resImporter = taskConfig.ResImporter;
            settings.assetImporter = taskConfig.AssetImporter;
            settings.assetCreator = taskConfig.AssetCreator;
            settings.excelImporter = taskConfig.ExcelImporter;
            settings.protocImporter = taskConfig.ProtocImporter;
            
            settings.assetBundleBuilder = taskConfig.AssetBundleBuilder;
            settings.dllBuilder = taskConfig.DllBuilder;
            settings.dataBuilder = taskConfig.DataBuilder;
            
            settings.dlcBuilder = taskConfig.DLCBuilder;
            
            settings.buildPlayer = taskConfig.BuildPlayer;
            settings.buildProject = taskConfig.BuildProject;
            settings.SaveEx();
            
            Instance.BuildSettings();
        }

    }
}