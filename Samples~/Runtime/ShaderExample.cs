using UnityEngine;

namespace EasyFramework.Samples.Modules
{
    public static class ShaderExample
    {
        // 同步调用前先等待 F.InitializeAsync()；资源组仍只在业务启动时加载一次。
        public static Shader Load(string resourceName, string shaderName)
        {
            F.ShaderLoader.Load(resourceName);
            return GetShader(shaderName);
        }

        public static Shader GetShader(string shaderName) => F.ShaderLoader.GetShader(shaderName);

        // 先准备 MyShaders 资源及 My/Shader；同组资源只在启动时加载一次，当前无卸载接口。
        public static async ETask<Shader> LoadAsync(string resourceName = "MyShaders", string shaderName = "My/Shader")
        {
            await F.InitializeAsync();
            await F.ShaderLoader.LoadAsync(resourceName);
            return GetShader(shaderName);
        }
    }
}
