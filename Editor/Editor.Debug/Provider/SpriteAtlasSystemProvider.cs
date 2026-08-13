/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class SpriteAtlasSystemProvider : ProjectSettingsProvider<SpriteAtlasSystemProvider>
    {
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public SpriteAtlasSystemProvider() : base(EasyFrameworkProvider.ToChildProvider<SpriteLoader>()) { }

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

            if (!SpriteLoader.HasInstance()) return;
            
            foreach (var value in SpriteLoader.Instance.AtlasDict.Values)
            {
                EditorGUILayout.LabelField($"{value.AtlasName}", $"Alive: {value.Alive}");
            }
        }
    }
}