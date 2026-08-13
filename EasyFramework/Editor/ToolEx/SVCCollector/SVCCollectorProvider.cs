/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class SVCCollectorProvider : ProjectSettingsProvider<SVCCollectorProvider>
    {
        private const string SettingsPath = ToolProvider.SettingPath + "/" + nameof(SVCCollector);
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public SVCCollectorProvider() : base(SettingsPath) { }

        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                SVCCollectorSettings.CreateInstance(),
            };
        }

        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);
            
            var settings = SVCCollectorSettings.Instance;
            var newVal = EditorGUILayout.TextField("SvcFileName", settings.SvcFileName);
            if (newVal != settings.SvcFileName)
            {
                settings.SvcFileName = newVal;
            }
            EditorGUILayout.LabelField("SvcSaveFile", settings.SvcSaveFile);
            
            ToolDrawHelper.DrawExtensions(SVCCollector.Instance.Extensions);
        }
    }
}