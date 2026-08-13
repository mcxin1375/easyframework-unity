/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class DingTalkExtensionProvider : ProjectSettingsProvider<DingTalkExtensionProvider>
    {
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public DingTalkExtensionProvider() : base(EasyFrameworkEditorProvider.ToChildProvider(nameof(DingTalkExtensionSettings))) { }

        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                DingTalkExtensionSettings.CreateInstance(),
            };
        }

        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);

            if (GUILayout.Button("Send Message Test",  GUILayout.Width(150),  GUILayout.Height(30)))
            {
                DingTalkExtension.SendMessage("DingTalkExtension", "UnityEditor Test");
            }
        }
    }
}