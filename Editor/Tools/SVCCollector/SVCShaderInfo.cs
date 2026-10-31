/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/1/30
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyFramework.Editor
{
    public class SVCShaderInfo
    {
        public readonly string ShaderName;

        public Dictionary<string, ShaderVariantCollection.ShaderVariant> ShaderVariantDict = new();

        public SVCShaderInfo(string name)
        {
            ShaderName = name;
        }

        public bool Contains(ShaderSnippetData snippet, ShaderCompilerData compilerData) => Contains(snippet.passType, compilerData.shaderKeywordSet.GetShaderKeywords());
        public bool Contains(PassType passType, string[] keywords)
        {
            var key = SVCHelper.ShaderVariantToString(ShaderName, passType.ToString(), keywords);
            return ShaderVariantDict.ContainsKey(key);
        }
        public bool Contains(PassType passType, ShaderKeyword[] keywords)
        {
            var key = SVCHelper.ShaderVariantToString(ShaderName, passType.ToString(), keywords);
            return ShaderVariantDict.ContainsKey(key);
        }

        public void AddVariantInfo(ShaderVariantCollection.ShaderVariant shaderVariant)
        {
            var key = SVCHelper.ShaderVariantToString(ShaderName, shaderVariant.passType.ToString(), shaderVariant.keywords);
            if (!ShaderVariantDict.ContainsKey(key))
            {
                ShaderVariantDict.Add(key, shaderVariant);
            }
        }
    }
}