/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    public interface IShaderLoader
    {
        void Load(string abName);
        ETask LoadAsync(string abName);
        Shader GetShader(string shaderName);
    }
}