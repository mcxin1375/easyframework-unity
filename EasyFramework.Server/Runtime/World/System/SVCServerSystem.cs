/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2026/1/5
// describe:
//----------------------------------------------------------------*/

using EasyFramework.Profiler;

namespace EasyFramework.Server
{
    public class SVCServerSystem : FSystem
    {
        protected override void OnCreate()
        {
            FProfiler.ProfilerSystem.OnShaderVariantError += OnShaderVariantError;
        }

        protected override void OnDestroy()
        {
            FProfiler.ProfilerSystem.OnShaderVariantError -= OnShaderVariantError;
        }

        private void OnShaderVariantError(ShaderVariantInfo shaderVariantInfo)
        {
            if (!FServer.Settings.svcServerSystem) return;
            
            _ = ServerAPI.UploadSVCErrorAsync(shaderVariantInfo.Log);
        }
    }
}