/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class SVCCollectorProvider : ProjectSettingsProvider<SVCCollectorSettings>
    {
        private const string SettingsPath = ToolsProvider.SettingPath + "/" + nameof(SVCCollector);
        
        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<SVCCollectorProvider>.Instance;
        
        public SVCCollectorProvider() : base(SettingsPath) { }


        protected override void OnDrawSettingsAfter(string searchContext)
        {
            var settings = SVCCollectorSettings.Instance;
            var newVal = EditorGUILayout.TextField("SvcFileName", settings.SvcFileName);
            if (newVal != settings.SvcFileName)
            {
                settings.SvcFileName = newVal;
            }
            EditorGUILayout.LabelField("SvcSaveFile", settings.SvcSaveFile);
        }
    }
}