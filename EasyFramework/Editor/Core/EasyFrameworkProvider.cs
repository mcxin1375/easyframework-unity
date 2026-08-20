/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class EasyFrameworkProvider : ProjectSettingsProvider<EasyFrameworkProvider>
    {
        public const string SettingPath = "Project/EasyFramework";
        
        protected override bool DrawTab => false;
        
        private string[] _appSelects;
        private int _appIndex;
        
        private static readonly Type[] AppTypes = EasyFrameworkReflection.FindInstanceTypes<IApp>();

        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        
        public EasyFrameworkProvider() : base(SettingPath) { }
        public static string ToChildProvider(string providerName) => $"{SettingPath}/{providerName}";
        public static string ToChildProvider<T>() => $"{SettingPath}/{typeof(T).Name}";

        
        
        protected override ScriptableObject[] LoadObjects()
        {
            _appSelects = AppTypes.Select(item => item.FullName).ToArray();
            _appIndex = 0;
            for (int i = 0; i < _appSelects.Length; i++)
            {
                if (_appSelects[i] == EasyFrameworkSettings.App.GetType().FullName)
                {
                    _appIndex = i;
                    break;
                }
            }
            
            return new ScriptableObject[]
            {
                EasyFrameworkSettings.Instance,
                EasyFrameworkEditorSettings.CreateInstance()
            };
        }
        

        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);
            
            var app = EasyFrameworkSettings.App;
            EditorGUILayout.HelpBox($"{nameof(IApp)}: {app?.GetType().Name}", MessageType.Info);
            if (app != null)
            {
                EditorGUILayout.LabelField("AppName", app.AppName);
                EditorGUILayout.LabelField("MainVersion", $"{app.MainVersion}");
                EditorGUILayout.LabelField("BundleVersion", app.BundleVersion);
                EditorGUILayout.LabelField("BundleIdentifier", app.BundleIdentifier);
                EditorGUILayout.LabelField("AppVersionFileUrl", app.AppVersionFileUrl);
                EditorGUILayout.LabelField("DLCPlatformServerUrl", app.DLCPlatformServerUrl);
            }
        }
        
        protected override void OnSettingsChanged(string settingsName)
        {
            base.OnSettingsChanged(settingsName);

            if (Application.isPlaying)
            {
                FDebug.DebugLevel = EasyFrameworkSettings.Instance.debugLevel;
            }
        }

        private void SelectApp(Type app)
        {
            StringBuilder sb = new();
            var str = EasyFrameworkSettings.App.AppSymbols?.Length > 0 ? string.Join(", ", EasyFrameworkSettings.App.AppSymbols) : string.Empty;
            FDebug.Log($"{EasyFrameworkSettings.App.AppName}: {str}");
            if (EasyFrameworkSettings.App.AppSymbols?.Length > 0)
            {
                foreach (var symbol in EasyFrameworkSettings.App.AppSymbols) sb.AppendLine($"-define:{symbol}");
            }
            
            string cscFile = "Assets/csc.rsp";
            File.WriteAllText(cscFile, sb.ToString());
            AssetDatabase.Refresh();
        }
    }
}