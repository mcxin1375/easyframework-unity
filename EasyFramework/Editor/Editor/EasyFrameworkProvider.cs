/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class EasyFrameworkProvider : ProjectSettingsProvider<EasyFrameworkProvider>
    {
        protected override bool DrawTab => false;

        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();

        public const string SettingPath = "Project/EasyFramework";
        
        
        public EasyFrameworkProvider() : base(SettingPath) { }
        public static string ToChildProvider(string providerName) => $"{SettingPath}/{providerName}";
        public static string ToChildProvider<T>() => $"{SettingPath}/{typeof(T).Name}";

        
        
        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                EasyFrameworkSettings.Instance,
                EasyFrameworkEditorSettings.CreateInstance()
            };
        }
        

        protected override void OnAfterDrawSettings(string settingsName)
        {
        }

        protected override void OnAfterDraw()
        {
            base.OnAfterDraw();
            
            // EditorGUILayout.HelpBox($"{nameof(EasyFrameworkReflection)}", MessageType.Info);
            // foreach (var assembly in EasyFrameworkReflection.RegisterAssemblies)
            // {
            //     EditorGUILayout.LabelField($"{assembly.GetName().Name}");
            // }
        }
    }
}