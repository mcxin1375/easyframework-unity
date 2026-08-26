/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public static class EasyFrameworkEditorMenuItem
    {
        private const int Priority = MenuItemOrder.Editor;
        
        [MenuItem("EasyFramework/Settings...", priority = 0)]
        public static void OpenSettings() => SettingsService.OpenProjectSettings(EasyFrameworkProvider.SettingPath);
        
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