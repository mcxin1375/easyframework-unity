/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class SVNExtensionProvider : ProjectSettingsProvider<SVNExtensionProvider>
    {
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public SVNExtensionProvider() : base(EasyFrameworkEditorProvider.ToChildProvider(nameof(SVNExtensionSettings))) { }

        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                SVNExtensionSettings.CreateInstance(),
            };
        }

        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);
            
        }
    }
}