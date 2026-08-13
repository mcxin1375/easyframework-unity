/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class EasyFrameworkEditorProvider : ProjectSettingsProvider<EasyFrameworkEditorProvider>
    {
        public const string SettingPath = "Project/EasyFramework Editor";
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();

        public EasyFrameworkEditorProvider() : base(SettingPath)
        {
        }
        public static string ToChildProvider(string providerName) => $"{SettingPath}/{providerName}";

        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                // EasyFrameworkToolSettings.Load(),
            };
        }

        protected override void OnAfterDraw()
        {
            EditorGUILayout.HelpBox($"EasyFramework Tools", MessageType.Info);
        }
    }
}