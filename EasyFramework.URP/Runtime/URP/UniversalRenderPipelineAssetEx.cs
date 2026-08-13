/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/24
// describe:
//----------------------------------------------------------------*/

using System.Reflection;
using UnityEngine.Rendering.Universal;

namespace EasyFramework.URP
{
    public static class UniversalRenderPipelineAssetEx
    {
        
        public static ScriptableRendererData[] GetScriptableRendererDataArrayEx(this UniversalRenderPipelineAsset asset)
        {
            if (asset == null) return null;
            
            var info = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (info == null) return null;

            var list = info.GetValue(asset);
            if (list is ScriptableRendererData[] arr) return arr;
            return null;
        }
        
        public static ScriptableRendererData GetScriptableRendererDataEx(this UniversalRenderPipelineAsset asset, int index)
        {
            if (asset == null || index < 0) return null;
            
            var info = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (info == null) return null;

            var list = info.GetValue(asset);
            if (list is ScriptableRendererData[] arr)
            {
                return arr.Length > index ? arr[index] : null;
            }

            return null;
        }
        
        public static UniversalRendererData GetUniversalRendererDataEx(this UniversalRenderPipelineAsset asset, int index)
        {
            var data = GetScriptableRendererDataEx(asset, index);
            if (data is UniversalRendererData universalRendererData) return universalRendererData;
            return null;
        }
    }
}
