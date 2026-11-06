/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public static class EasyFrameworkMenuItem
    {
        private const int Priority = MenuItemOrder.Base;
        
        [MenuItem("EasyFramework/About", priority = Priority)]
        public static void OpenFrameworkURL() => Application.OpenURL(EasyFrameworkPreferences.FrameworkURL);
        
        // [MenuItem("EasyFramework/Github", priority = 0)]
        // public static void OpenServerUrl() => Application.OpenURL(EasyFrameworkPreferences.GithubURL);
        
        [MenuItem("EasyFramework/Settings...", priority = Priority + 100)]
        public static void OpenSettings() => SettingsService.OpenProjectSettings(EasyFrameworkProvider.SettingPath);
        
        [MenuItem("EasyFramework/Preferences...", priority = Priority + 100)]
        public static void OpenPreferences() => SettingsService.OpenUserPreferences(EasyFrameworkPreferences.SettingPath);
    }
}