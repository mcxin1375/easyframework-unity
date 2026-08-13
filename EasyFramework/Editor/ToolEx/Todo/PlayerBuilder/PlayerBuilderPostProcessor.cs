using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class PlayerBuilderPostProcessor
    {
        [PostProcessBuild(2000)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (!PlayerBuilderSettings.Instance.cleanupTempDir) return;
            try
            {
                var outputFolder = Path.GetDirectoryName(path);
                var delDir = $"{outputFolder}/{PlayerSettings.productName}_BurstDebugInformation_DoNotShip";
                Debug.Log($"{delDir}");
                FileHelper.DeleteDirectory(delDir);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"An unexpected exception occurred while performing build cleanup: {e}");
            }
        }
    }
}