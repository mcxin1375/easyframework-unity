/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public static class EasyFrameworkEditorMenuItemOrder
    {
        public const int Editor = 1000;
        public const int Tools = 2000;
        public const int ToolsEx = 3000;
        public const int Server = 4000;
        public const int Window = 5000;
    }

    public static class EasyFrameworkEditorMenuItem
    {
        private const int Priority = EasyFrameworkEditorMenuItemOrder.Editor;
        
        [MenuItem("EasyFramework/Settings...", priority = 0)]
        public static void OpenSettings() => SettingsService.OpenProjectSettings(EasyFrameworkAOTProvider.SettingPath);
        
        [MenuItem("EasyFramework/Preferences...", priority = 0)]
        public static void OpenPreferences() => SettingsService.OpenUserPreferences(EasyFrameworkPreferences.SettingPath);
        
        [MenuItem("EasyFramework/Open ServerUrl", priority = 0)]
        public static void OpenServerUrl() => Application.OpenURL(EasyFrameworkPreferences.ServerUrl);

        [MenuItem("EasyFramework/Editor/PersistentDataPath(Show in Explorer)", priority = Priority + 200)]
        private static void PersistentDataPath()
        {
            System.Diagnostics.Process.Start(Application.persistentDataPath);
        }
        [MenuItem("EasyFramework/Editor/PersistentDataPath(Clear)", priority = Priority + 200)]
        private static void PersistentDataPathClear()
        {
            FileHelper.ClearDirectory(Application.persistentDataPath);
        }
        [MenuItem("EasyFramework/Editor/PlayerPrefs-DeleteAll", false, priority = Priority + 300)]
        public static void PlayerPrefsDeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }
        [MenuItem("EasyFramework/Editor/EditorPrefs-DeleteAll", false, priority = Priority + 300)]
        public static void EditorPrefsDeleteAll()
        {
            EditorPrefs.DeleteAll();
        }
        
    }
}