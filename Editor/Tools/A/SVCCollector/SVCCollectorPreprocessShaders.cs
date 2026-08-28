/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;

namespace EasyFramework.Editor
{

    class SVCCollectorPreprocessShadersBefore : IToolEvent<AssetBundleBuilder>
    {
        public int Order => AssetBundleBuilder.Instance.Order - 1;
        public void OnExecute()
        {
            if (SVCCollectorSettings.Instance.preprocessShaders)
            {
                SVCCollectorPreprocessShaders.Enabled = true;
                SVCCollectorPreprocessShaders.ShaderBuildInfo = SVCCollectorUtility.CreateShaderVariantCollectionInfo();
                SVCCollectorPreprocessShaders.ShaderVariantsBuildList.Clear();
            }
        }
    }
    
    class SVCCollectorPreprocessShadersAfter : IToolEvent<AssetBundleBuilder>
    {
        public int Order => AssetBundleBuilder.Instance.Order + 1;
        public void OnExecute()
        {
            if (SVCCollectorSettings.Instance.preprocessShaders)
            {
                CreateDebugText();
                
                SVCCollectorPreprocessShaders.Enabled = false;
                SVCCollectorPreprocessShaders.ShaderVariantsBuildList.Clear();
            }
        }

        private void CreateDebugText()
        {
            StringBuilder sb = new StringBuilder();

            var shaderBuilderInfo = SVCCollectorPreprocessShaders.ShaderBuildInfo;
            if (shaderBuilderInfo != null)
            {
                sb.AppendLine("---------------------------------- BuildAllVariantsShaderHashSet");
                foreach (var s in shaderBuilderInfo.BuildAllVariantsShaderHashSet) sb.AppendLine(s);
                sb.AppendLine("---------------------------------- ShaderVariantsHashSet");
                foreach (var s in shaderBuilderInfo.ShaderVariantsHashSet) sb.AppendLine(s);
            }

            sb.AppendLine("---------------------------------- ShaderVariantsBuildList");
            foreach (string s in SVCCollectorPreprocessShaders.ShaderVariantsBuildList) sb.AppendLine(s);

            FileHelper.CreateDirectory(AssetBundleBuilder.Instance.DebugPlatformPath);
            string debugFile = $"{AssetBundleBuilder.Instance.DebugPlatformPath}/ShaderVariantsBuildDebug.txt";
            File.WriteAllText(debugFile, sb.ToString());
        }
    }
    
    class SVCCollectorPreprocessShaders : IPreprocessShaders
    {
        public static readonly List<string> ShaderVariantsBuildList = new();
        public static SVCInfo ShaderBuildInfo;

        public int callbackOrder => 0;
        public static bool Enabled = false;

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            if (!Enabled) return;

            // Debug.Log($"OnProcessShader Start: {shader.name} : {snippet.passType} : {data.Count}");

            ShaderVariantsBuildList.Add($"----------- OnProcessShader Start: {shader.name} : {snippet.passType} : {data.Count}");

            if (ShaderBuildInfo != null)
            {
                for (int i = data.Count - 1; i >= 0; i--)
                {
                    ShaderCompilerData cd = data[i];

                    var str = SVCHelper.ShaderVariantToString(shader, snippet, cd);
                    bool contains = ShaderBuildInfo.Contains(shader, snippet, cd);
                    if (!contains) data.RemoveAt(i);
                    else
                    {
                        ShaderVariantsBuildList.Add(str);
                        // Debug.Log(str);
                    }
                }
            }

            ShaderVariantsBuildList.Add($"----------- OnProcessShader End: {shader.name} : {snippet.passType} : {data.Count}");
            // Debug.Log($"OnProcessShader End: {shader.name} : {snippet.passType} : {data.Count}");
        }
    }
}