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
            
//             EditorGUILayout.HelpBox($"{nameof(ResPoolBehaviour)}", MessageType.Info);
//             foreach (var kv in F.ResLoader.ResPools)
//             {
//                 EditorGUILayout.LabelField($"{kv.ResName}", $"CreatedCount: {kv.CreatedCount}, PooledCount: {kv.PooledCount}");
//             }
            
            EditorGUILayout.HelpBox("WorldManager", MessageType.Info);
            foreach (var world in WorldManager.Instance.WorldList)
            {
                EditorGUILayout.LabelField($"World: {world.Index}");
                foreach (var val in world.SystemList)
                {
                    EditorGUILayout.LabelField($"{val.Order}", val.GetType().Name);
                }
            }
            
            EditorGUILayout.HelpBox("ControllerManager", MessageType.Info);
            foreach (var controller in ControllerManager.Instance.EnterList)
            {
                EditorGUILayout.LabelField($"{controller.GetType().Name}", $"IsEnter: {controller.IsEnter}, IsActive: {controller.IsActive}");
            }
            
            EditorGUILayout.HelpBox("SceneLoader", MessageType.Info);
            foreach (var value in SceneLoader.Instance.SceneDict.Values)
            {
                EditorGUILayout.LabelField($"{value.SceneName}", $"State: {value.State}, IsActive: {value.IsActive}, Alive: {value.Alive}");
            }
            
            EditorGUILayout.HelpBox("SpriteLoader", MessageType.Info);
            foreach (var value in SpriteLoader.Instance.AtlasDict.Values)
            {
                EditorGUILayout.LabelField($"{value.AtlasName}", $"Alive: {value.Alive}");
            }
        }
        
    }
}