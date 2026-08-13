/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;

namespace EasyFramework
{
    public class ShaderLoader : Singleton<ShaderLoader>, IShaderLoader, IResRequest
    {
        public bool Alive { get; } = true;
        private readonly Dictionary<string, Shader> _shaderDict = new();
        
        public void Load(string abName)
        {
            FDebug.Log($"ShaderSystem.LoadShader(abName = {abName})");
            var arr = F.ResLoader.LoadAllAssets<Shader>(abName, this);
            if (arr?.Length > 0)
            {
                foreach (Shader shader in arr)
                {
                    // Debug.Log($"Add Shader: {shader.name}");
                    TryAdd(shader);
                }
            }
        }
        
        public async ETask LoadAsync(string abName)
        {
            FDebug.Log($"ShaderSystem.LoadShaderAsync(abName = {abName})");
            var arr = await F.ResLoader.LoadAllAssetsAsync<Shader>(abName, this);
            if (arr?.Length > 0)
            {
                foreach (Shader shader in arr)
                {
                    TryAdd(shader);
                }
            }
        }
        
        public Shader GetShader(string shaderName)
        {
#if UNITY_EDITOR
            if (_shaderDict.ContainsKey(shaderName)) return Shader.Find(shaderName);
            return Shader.Find(shaderName);
#endif
            
            if (_shaderDict.TryGetValue(shaderName, out var value)) return value;
            return Shader.Find(shaderName);
        }

        private void TryAdd(Shader shader)
        {
            if (!_shaderDict.TryAdd(shader.name, shader))
            {
                FDebug.LogError($"ShaderManager shader name repeated. name: {shader.name}");
            }
        }
        
    }
}