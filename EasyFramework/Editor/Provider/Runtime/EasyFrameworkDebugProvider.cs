/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class EasyFrameworkDebugProvider : ProjectSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<EasyFrameworkDebugProvider>.Instance;
        
        public EasyFrameworkDebugProvider() : base(EasyFrameworkProvider.ToChildProvider(". Debug")) { }

        protected override void OnDrawSettings(string searchContext)
        {
            // EditorGUILayout.HelpBox("Type - (PooledCount / CreatedCount)", MessageType.Info);
            EditorGUILayout.LabelField("ThreadId", $"{ETask.ThreadId}");
            EditorGUILayout.LabelField("ThreadType", $"{ETask.ThreadType.Name}");
            // EditorGUILayout.LabelField("Time", $"{EasyTask.Timer.Time}");
            
            EditorGUILayout.HelpBox("ObjectPool", MessageType.Info);
            foreach (var objectPool in FDebug.PoolDebugListRO)
            {
                EditorGUILayout.LabelField($"{objectPool.ObjectType.FullName} - ({objectPool.PooledCount} / {objectPool.CreatedCount})");
            }
            
            FBehaviourInspector.DrawInfo();
        }
    }
}