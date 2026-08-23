/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class AssetBundleProvider : ProjectSettingsProvider
    {
        private bool _loaded = true;
        private bool _deps = true;

        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<AssetBundleProvider>.Instance;
        public AssetBundleProvider() : base(EasyFrameworkProvider.ToChildProvider<AssetBundle>()) { }

        protected override void OnDrawSettingsAfter(string searchContext)
        {
            _loaded = EditorGUILayout.Toggle("Loaded", _loaded);
            _deps = EditorGUILayout.Toggle("Deps", _deps);
            
            if (AssetBundleLoader.HasInstance())
            {
                foreach (var bundle in AssetBundleLoader.Instance.AbDict.Values)
                {
                    if (_loaded && bundle.Bundle == null) continue;

                    EditorGUILayout.LabelField($"{GUIStyles.MainPrefix}{bundle.FileName}", GUIStyles.MainStyle);

                    if (_deps)
                    {
                        var dependencies = AssetBundleLoader.Instance.GetAllDependencies(bundle.FileName);
                        if (dependencies?.Length > 0)
                        {
                            foreach (var dep in dependencies)
                            {
                                EditorGUILayout.LabelField($"{GUIStyles.DependencyPrefix}{dep}", GUIStyles.DependencyStyle);
                            }
                        }
                    }

                    EditorGUILayout.Space(5);
                }
            }
        }
    }
}