/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

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

            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            var bundleIdentifier = PlayerSettings.GetApplicationIdentifier(namedTarget);
            var buildPlayerName = $"{bundleIdentifier}_{PlayerSettings.bundleVersion}";
             
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
            
            var basePath = PlayerBuilder.Instance.ProjectPlatformPath;
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    options.locationPathName = $"{basePath}/{buildPlayerName}({PlayerSettings.Android.bundleVersionCode}).apk";
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
                        options.locationPathName = $"{PlayerBuilder.Instance.ProjectPlatformPath}/Project_{buildPlayerName}";
                    }
                    break;
            }

            return options;
        }
    }
}