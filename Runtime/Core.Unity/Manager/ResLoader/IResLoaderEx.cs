using UnityEngine;

namespace EasyFramework
{
    public static class IResLoaderEx
    {
        public static GameObject CreateObj(this IResLoader resLoader, string resName, Transform parent = null, IResRequest request = null)
        {
            var go = resLoader.LoadAsset<GameObject>(resName, request);
            if (go == null)
            {
                FDebug.LogError($"CreateObj [{resName}] is empty!");
                return null;
            }
            
#if UNITY_EDITOR
            if (resLoader is AssetBundleLoader)
            {
                var obj = Object.Instantiate(go, parent);
                AssetBundleHelper.ResetShaderEditorOnly(obj);
                return obj;
            }
#endif
            
            return Object.Instantiate(go, parent);
        }
        
        public static async ETask<GameObject> CreateObjAsync(this IResLoader resLoader, string resName, Transform parent = null, IResRequest request = null)
        {
            var go = await resLoader.LoadAssetAsync<GameObject>(resName, request);
            if (go == null)
            {
                FDebug.LogError($"CreateObjAsync [{resName}] is empty!");
                return null;
            }
            
#if UNITY_EDITOR
            if (resLoader is AssetBundleLoader)
            {
                var obj = Object.Instantiate(go, parent);
                AssetBundleHelper.ResetShaderEditorOnly(obj);
                return obj;
            }
#endif
            
            return Object.Instantiate(go, parent);
        }
        
        public static async ETask<GameObject[]> CreateObjAsync(this IResLoader resLoader, string resName, int count, Transform parent = null, IResRequest request = null)
        {
            var go = await resLoader.LoadAssetAsync<GameObject>(resName, request);
            if (go == null)
            {
                FDebug.LogError($"CreateObjAsync [{resName}] is empty!");
                return null;
            }
            
#if UNITY_2022_3_OR_NEWER

            var asyncInstantiateOperation = Object.InstantiateAsync(go, count, parent);
            if (asyncInstantiateOperation == null) return null;
            
#if UNITY_EDITOR
            if (resLoader is AssetBundleLoader)
            {
                await asyncInstantiateOperation;
                foreach (var obj in asyncInstantiateOperation.Result) AssetBundleHelper.ResetShaderEditorOnly(obj);
            }
#endif
            return asyncInstantiateOperation.Result;
            
#else
            
            var arr = new GameObject[count];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = Object.Instantiate(go, parent);
                
#if UNITY_EDITOR
                if (resLoader is AssetBundleLoader)
                {
                    AssetBundleHelper.ResetShaderEditorOnly(arr[i]);
                }
#endif
                
            }
            return arr;
#endif
            
        }
    }
}