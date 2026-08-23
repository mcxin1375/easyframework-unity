/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class SceneSystemProvider : ProjectSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<SceneSystemProvider>.Instance;
        
        public SceneSystemProvider() : base(EasyFrameworkProvider.ToChildProvider<SceneLoader>()) { }


        protected override void OnDrawSettings(string searchContext)
        {
            if (!SceneLoader.HasInstance()) return;
            
            foreach (var value in SceneLoader.Instance.SceneDict.Values)
            {
                EditorGUILayout.LabelField($"{value.SceneName}", $"State: {value.State}, IsActive: {value.IsActive}, Alive: {value.Alive}");
            }
        }
    }
}