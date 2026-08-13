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
    public class EasyFrameworkAOTProvider : ProjectSettingsProvider<EasyFrameworkAOTProvider>
    {
        public const string SettingPath = "Project/EasyFramework AOT";
        protected override bool DrawTab => false;
        
        private string[] _appSelects;
        private int _appIndex;
        
        private static readonly Type[] AppTypes = EasyFrameworkReflection.FindInstanceTypes<IApp>();
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();

        public EasyFrameworkAOTProvider() : base(SettingPath) { }

        protected override ScriptableObject[] LoadObjects()
        {
            _appSelects = AppTypes.Select(item => item.FullName).ToArray();
            _appIndex = 0;
            for (int i = 0; i < _appSelects.Length; i++)
            {
                if (_appSelects[i] == EasyFrameworkAOTSettings.App.GetType().FullName)
                {
                    _appIndex = i;
                    break;
                }
            }
            
            return new ScriptableObject[]
            {
                EasyFrameworkAOTSettings.CreateInstance(),
            };
        }
        
        protected override void OnBeforeDrawSettings(string settingsName)
        {
            base.OnBeforeDrawSettings(settingsName);
            
            var appIndex = EditorGUILayout.Popup($"App", _appIndex, _appSelects);
            if (appIndex != _appIndex)
            {
                SelectApp(AppTypes[appIndex]);
            }
        }

        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);
            
            var app = EasyFrameworkAOTSettings.App;
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

        protected override void OnAfterDraw()
        {
            base.OnAfterDraw();
            
            // EditorGUILayout.HelpBox($"{nameof(EasyFrameworkAOTReflection)}", MessageType.Info);
            // foreach (var assembly in EasyFrameworkAOTReflection.TagAssemblies)
            // {
            //     EditorGUILayout.LabelField($"{assembly.GetName().Name}");
            // }
        }

        protected override void OnSettingsChanged(string settingsName)
        {
            base.OnSettingsChanged(settingsName);

            if (Application.isPlaying)
            {
                FDebug.DebugLevel = EasyFrameworkAOTSettings.Instance.debugLevel;
            }
        }

        private void SelectApp(Type app)
        {
            StringBuilder sb = new();
            var str = EasyFrameworkAOTSettings.App.AppSymbols?.Length > 0 ? string.Join(", ", EasyFrameworkAOTSettings.App.AppSymbols) : string.Empty;
            FDebug.Log($"{EasyFrameworkAOTSettings.App.AppName}: {str}");
            if (EasyFrameworkAOTSettings.App.AppSymbols?.Length > 0)
            {
                foreach (var symbol in EasyFrameworkAOTSettings.App.AppSymbols) sb.AppendLine($"-define:{symbol}");
            }
            
            string cscFile = "Assets/csc.rsp";
            File.WriteAllText(cscFile, sb.ToString());
            AssetDatabase.Refresh();
        }
    }
}