/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class EasyTaskProvider : ProjectSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<EasyTaskProvider>.Instance;
        
        public EasyTaskProvider() : base(EasyFrameworkProvider.ToChildProvider(". EasyTask")) { }


        protected override void OnDrawSettings(string searchContext)
        {
            // EditorGUILayout.HelpBox("Type - (PooledCount / CreatedCount)", MessageType.Info);
            
            EditorGUILayout.LabelField("ThreadId", $"{ETask.ThreadId}");
            EditorGUILayout.LabelField("ThreadType", $"{ETask.ThreadType.Name}");
            // EditorGUILayout.LabelField("Time", $"{EasyTask.Timer.Time}");
        }
        
    }
}