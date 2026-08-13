/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/8/14
// describe:
//----------------------------------------------------------------*/

using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyFramework
{
    public static partial class UnityHelper
    {
        public static GameObject CreateObjEx(this AssetBundle assetBundle, string assetName, Transform parent = null)
        {
            GameObject go = assetBundle.LoadAsset<GameObject>(assetName);
            if (go == null)
            {
                FDebug.LogWarning($"AssetBundle [{assetBundle.name}] CreateObj Failed!");
                return null;
            }
            return Object.Instantiate(go, parent);
        }

#if UNITY_2022_3_OR_NEWER
        public static async ETask<GameObject> CreateObjAsyncEx(this AssetBundle assetBundle, string assetName, Transform parent = null)
        {
            GameObject[] result = await CreateObjAsyncEx(assetBundle, assetName, 1, parent);
            return result?.Length > 0 ? result[0] : null;
        }
        public static async ETask<GameObject[]> CreateObjAsyncEx(this AssetBundle assetBundle, string assetName, int count, Transform parent = null)
        {
            GameObject go = await assetBundle.LoadAssetAsyncEx<GameObject>(assetName);
            if (go == null)
            {
                FDebug.LogWarning($"ResLoader CreateAsync [{assetName}] Failed!");
                return null;
            }

            var asyncInstantiateOperation = Object.InstantiateAsync(go, count, parent);
            if (asyncInstantiateOperation == null) return null;
            await asyncInstantiateOperation;

            return asyncInstantiateOperation.Result;
        }
#endif

        public static async ETask<T> LoadAssetAsyncEx<T>(this AssetBundle assetBundle, string abName) where T : Object
        {
            var assetBundleRequest = assetBundle.LoadAssetAsync<T>(abName);
            if (assetBundleRequest == null) return null;
            await assetBundleRequest;

            if (assetBundleRequest.asset is T t) return t;
            return null;
        }
        public static async ETask<T[]> LoadAllAssetsAsyncEx<T>(this AssetBundle assetBundle) where T : Object
        {
            var assetBundleRequest = assetBundle.LoadAllAssetsAsync<T>();
            if (assetBundleRequest == null) return null;
            await assetBundleRequest;

            if (assetBundleRequest.allAssets is T[] t) return t;
            return null;
        }
        
    }
}