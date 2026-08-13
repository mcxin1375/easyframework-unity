/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class SceneSystemProvider : ProjectSettingsProvider<SceneSystemProvider>
    {
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public SceneSystemProvider() : base(EasyFrameworkProvider.ToChildProvider<SceneLoader>()) { }

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

            if (!SceneLoader.HasInstance()) return;
            
            foreach (var value in SceneLoader.Instance.SceneDict.Values)
            {
                EditorGUILayout.LabelField($"{value.SceneName}", $"State: {value.State}, IsActive: {value.IsActive}, Alive: {value.Alive}");
            }
        }
    }
}