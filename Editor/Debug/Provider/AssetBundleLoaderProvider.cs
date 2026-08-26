/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class AssetBundleLoaderProvider : ProjectSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider Create() => Singleton<AssetBundleLoaderProvider>.Instance;
        
        public AssetBundleLoaderProvider() : base(EasyFrameworkProvider.ToChildProvider<AssetBundleLoader>()) { }

        protected override void OnDrawSettings(string searchContext)
        {
            if (AssetBundleLoader.HasInstance())
            {
                foreach (var info in AssetBundleLoader.Instance.RequestDict.Values)
                {
                    if (!string.IsNullOrWhiteSpace(searchContext) && !info.AbName.ToLower().Contains(searchContext)) continue;

                    EditorGUILayout.LabelField($"{GUIStyles.MainPrefix}{info.AbName}, LifeTime: {info.AliveCountDownTime}", GUIStyles.MainStyle);
                    if (info.ReferList != null)
                    {
                        foreach (var handler in info.ReferList)
                        {
                            EditorGUILayout.LabelField($"{GUIStyles.DependencyPrefix}Refer: {handler.GetType().Name} - {handler.Alive}", GUIStyles.DependencyStyle);
                        }
                    }

                    EditorGUILayout.Space(5);
                }
            }
        }
    }
}