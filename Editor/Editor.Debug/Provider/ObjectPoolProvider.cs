/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class ObjectPoolProvider : ProjectSettingsProvider<ObjectPoolProvider>
    {
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public ObjectPoolProvider() : base(EasyFrameworkProvider.ToChildProvider(". ObjectPool")) { }

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

            if (!Application.isPlaying) return;

            EditorGUILayout.HelpBox("Type - (PooledCount / CreatedCount)", MessageType.Info);
            
            foreach (var objectPool in FDebug.PoolDebugListRO)
            {
                EditorGUILayout.LabelField($"{objectPool.ObjectType.FullName} - ({objectPool.PooledCount} / {objectPool.CreatedCount})");
            }
            
            // EditorGUILayout.HelpBox("Item Debug", MessageType.Info);
            // foreach (var debug in ObjectPool.DebugList)
            // {
            //     EditorGUILayout.LabelField($"{debug.GetType().Name}", debug.GetDebugText(), EditorStyles.wordWrappedLabel);
            // }
        }
        
    }
}