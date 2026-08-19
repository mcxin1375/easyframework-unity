/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class ToolProvider : ProjectSettingsProvider<ToolProvider>
    {
        public const string SettingPath = "Project/EasyFramework Tools";
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();

        public ToolProvider() : base(SettingPath)
        {
        }

        protected override ScriptableObject[] LoadObjects()
        {
            ToolImporter.Instance.RefreshExtensions();
            ToolCreator.Instance.RefreshExtensions();
            ToolBuilder.Instance.RefreshExtensions();
            ToolPublisher.Instance.RefreshExtensions();
            
            return null;
        }

        protected override void OnAfterDraw()
        {
            ToolDrawHelper.DrawToolEvents(ToolImporter.Instance.ToolEvents);
            ToolDrawHelper.DrawToolEvents(ToolCreator.Instance.ToolEvents);
            ToolDrawHelper.DrawToolEvents(ToolBuilder.Instance.ToolEvents);
            ToolDrawHelper.DrawToolEvents(ToolPublisher.Instance.ToolEvents);
        }
    }
}