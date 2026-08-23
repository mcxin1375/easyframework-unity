/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class SpriteAtlasSystemProvider : ProjectSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<SpriteAtlasSystemProvider>.Instance;
        
        public SpriteAtlasSystemProvider() : base(EasyFrameworkProvider.ToChildProvider<SpriteLoader>()) { }

        protected override void OnDrawSettings(string searchContext)
        {
            if (!SpriteLoader.HasInstance()) return;
            
            foreach (var value in SpriteLoader.Instance.AtlasDict.Values)
            {
                EditorGUILayout.LabelField($"{value.AtlasName}", $"Alive: {value.Alive}");
            }
        }
    }
}