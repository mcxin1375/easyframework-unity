using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EasyFramework
{
    public static class AssetBundleHelper
    {
        
        public static string NameToKey(string abName) => abName.EndsWith(EasyFrameworkSettings.Instance.abSuffix)
            ? abName
            : $"{abName}{EasyFrameworkSettings.Instance.abSuffix}";
        
        public static string NameToURL(string abName)
        {
            return $"{EasyFrameworkSettings.Instance.DLCPath}/{abName}";
        }

#if UNITY_EDITOR


        internal static void ResetShaderEditorOnly(GameObject go)
        {
            // Log.Info(tgo.name);
            
            if (Application.isEditor)
            {
                // Log.Info("Renderer");
                foreach (Renderer element in go.GetComponentsInChildren<Renderer>(true))
                {
                    foreach (Material mat in element.sharedMaterials)
                    {
                        if (mat == null)
                        {
                            //Debug.LogError("mat is null 201509062008");
                            continue;
                        }
                        // Log.Info($"mat: {mat.name} shader: {mat.shader.name}");
                        
                        if (mat.isVariant)
                        {
                            mat.parent.shader = F.ShaderLoader.GetShader(mat.shader.name);
                        }
                        else
                        {
                            mat.shader = F.ShaderLoader.GetShader(mat.shader.name);
                        }
                    }
                }
                // Log.Info("Projector");
                foreach (Projector element in go.GetComponentsInChildren<Projector>(true))
                {
                    if (element.material != null)
                    {
                        element.material.shader = F.ShaderLoader.GetShader(element.material.shader.name);
                    }
                }
                // Log.Info("MaskableGraphic");
                foreach (MaskableGraphic element in go.GetComponentsInChildren<MaskableGraphic>(true))
                {
                    if (element.gameObject.GetComponent<TMPro.TMP_SubMeshUI>() != null)
                        continue;
                    
                    if (element.material != null)
                    {
                        element.material.shader = F.ShaderLoader.GetShader(element.material.shader.name);
                    }
                }

                // Log.Info("TMPro.TextMeshProUGUI");
                foreach (TMPro.TextMeshProUGUI element in go.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                {
                    // Log.Info(element.name);
                    // if (element.material != null)
                    // {
                    //     // Log.Info(element.material.shader.name, Shader.Find(element.material.shader.name));
                    //     element.material.shader = Shader.Find(element.material.shader.name);
                    // }
                    if (element.fontSharedMaterial != null)
                    {
                        // Log.Info($"{element.fontSharedMaterial.shader.name}, {Shader.Find(element.fontSharedMaterial.shader.name)}");
                        element.fontSharedMaterial.shader = F.ShaderLoader.GetShader(element.fontSharedMaterial.shader.name);
                    }
                }

                //foreach (ParticleSystem ps in tgo.GetComponentsInChildren<ParticleSystem>(true))
                //{
                //    foreach (Material mat in ps.ren.renderer.materials)
                //    {
                //        if (mat == null)
                //        {
                //            //Debug.LogError("mat is null 201509072008");
                //            continue;
                //        }
                //        mat.shader = Shader.Find("" + mat.shader.name);
                //    }
                //}
            }
        }

#endif
        
    }
}