/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEditor;

namespace EasyFramework.Editor
{
    public class BuildPlayerOptionsSettings : IPlayerBuilderSettings
    {
        public BuildPlayerOptions BuildPlayerOptions => GetBuildPlayerOptions();

        BuildPlayerOptions GetBuildPlayerOptions()
        {
            var settings = PlayerBuilderSettings.Instance;
            
            List<string> editorScenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                if (scene.enabled) editorScenes.Add(scene.path);

            string buildPlayerName = $"{EasyFrameworkSettings.App?.AppName ?? "-"}_{EasyFrameworkSettings.App?.MainVersion}";
             
            BuildPlayerOptions options = new BuildPlayerOptions();
            options.scenes = editorScenes.ToArray();
            options.target = EditorUserBuildSettings.activeBuildTarget;
            options.options = BuildOptions.None;
            
            if (settings.developmentBuild)
            {
                options.options |= BuildOptions.Development;
            }
            else if ((options.options & BuildOptions.Development) == BuildOptions.Development)
            {
                options.options -= BuildOptions.Development;
            }
            
            var basePath = PlayerBuilder.Instance.ProjectDataPath;
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    options.locationPathName = $"{basePath}/{buildPlayerName}.apk";
                    break;
                case BuildTarget.StandaloneWindows:
                    options.locationPathName = $"{basePath}/{buildPlayerName}/{buildPlayerName}.exe";
                    break;
                case BuildTarget.StandaloneWindows64:
                    options.locationPathName = $"{basePath}/{buildPlayerName}/{buildPlayerName}64.exe";
                    break;
                default:
                    options.locationPathName = $"{basePath}/{buildPlayerName}";
                    break;
            }
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    if (settings.exportAsGoogleAndroidProject)
                    {
                        options.locationPathName = $"{PlayerBuilder.Instance.ProjectDataPath}/Project_{buildPlayerName}";
                    }
                    break;
            }

            return options;
        }
    }
}