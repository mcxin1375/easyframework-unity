/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/24
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Rendering.Universal;

namespace EasyFramework.URP
{
    public static class ScriptableRendererEx
    {
        public static void AddRenderFeatureEx(this ScriptableRenderer scriptableRenderer, ScriptableRendererFeature rendererFeature)
        {
            var list = scriptableRenderer?.GetScriptableRendererFeatureListEx();
            if (list == null || rendererFeature == null) return;
            list.Add(rendererFeature);
        }
        
        public static List<ScriptableRendererFeature> GetScriptableRendererFeatureListEx(this ScriptableRenderer scriptableRenderer)
        {
            if (scriptableRenderer == null) return null;
            var fieldInfo = typeof(ScriptableRenderer).GetField("m_RendererFeatures", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var list = fieldInfo?.GetValue(scriptableRenderer);
            if (list is List<ScriptableRendererFeature> scriptableRendererFeatures) return scriptableRendererFeatures;
            return null;
        }

    }
}