/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/1/30
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class SVCInfo
    {
        public readonly HashSet<string> BuildAllVariantsShaderHashSet = new();
        
        public readonly List<ShaderVariantCollection.ShaderVariant> ShaderVariants = new();
        public IReadOnlyDictionary<string, SVCShaderInfo> ShaderDict => _shaderDict;
        private readonly Dictionary<string, SVCShaderInfo> _shaderDict = new();
        public readonly HashSet<string> ShaderVariantsHashSet = new();

        public SVCInfo(ShaderVariantCollection.ShaderVariant[] shaderVariants, Shader[] buildAllVariantsShaders = null)
        {
            _shaderDict.Clear();
            
            foreach (ShaderVariantCollection.ShaderVariant shaderVariant in shaderVariants)
            {
                var shader = shaderVariant.shader;
                if (!_shaderDict.ContainsKey(shader.name)) _shaderDict.Add(shader.name, new SVCShaderInfo(shader.name));
                SVCShaderInfo svcShaderInfo = _shaderDict[shader.name];
                svcShaderInfo.AddVariantInfo(shaderVariant);

                var str = SVCHelper.ShaderVariantToString(shaderVariant);
                ShaderVariantsHashSet.Add(str);
            }
            foreach (SVCShaderInfo shaderInfo in _shaderDict.Values)
            {
                ShaderVariants.AddRange(shaderInfo.ShaderVariantDict.Values.ToArray());
            }

            if (buildAllVariantsShaders?.Length > 0)
            {
                foreach (var shader in buildAllVariantsShaders) BuildAllVariantsShaderHashSet.Add(shader.name);
            }
        }

        public bool Contains(Shader shader, ShaderSnippetData snippet, ShaderCompilerData compilerData)
        {
            if (BuildAllVariantsShaderHashSet.Contains(shader.name)) return true;

            // if (_shaderDict.TryGetValue(shader.name, out var info) && info.Contains(snippet, compilerData)) return true;

            var str = SVCHelper.ShaderVariantToString(shader, snippet, compilerData);
            return ShaderVariantsHashSet.Contains(str);
        }
    }
}