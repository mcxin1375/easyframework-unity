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
        private const string SettingsPath = ToolProvider.SettingPath + "/" + nameof(SVNExtension);
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public SVNExtensionProvider() : base(SettingsPath) { }

        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                SVNExtensionSettings.CreateInstance(),
            };
        }
    }
}