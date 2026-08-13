/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class EasyTaskProvider : ProjectSettingsProvider<EasyTaskProvider>
    {
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public EasyTaskProvider() : base(EasyFrameworkProvider.ToChildProvider(". EasyTask")) { }

        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                // settings,
            };
        }

        protected override void OnAfterDraw()
        {
            base.OnAfterDraw();

            // EditorGUILayout.HelpBox("Type - (PooledCount / CreatedCount)", MessageType.Info);
            
            EditorGUILayout.LabelField("ThreadId", $"{ETask.ThreadId}");
            EditorGUILayout.LabelField("ThreadType", $"{ETask.ThreadType.Name}");
            // EditorGUILayout.LabelField("Time", $"{EasyTask.Timer.Time}");
        }
        
    }
}