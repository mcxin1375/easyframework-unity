// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2024/5/16
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using EasyFramework.Core;
// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;
//
// namespace EasyFramework.URP
// {
//     // [Serializable, VolumeComponentMenu("EasyFrameworkURP/GaussianBlur")]
//     // public class GaussianBlur : VolumeComponent, IPostProcessComponent
//     // {
//     //     public ClampedFloatParameter radius = new ClampedFloatParameter(0f, 0, 6, true);
//     //     public ClampedIntParameter iteration = new ClampedIntParameter(5, 1, 10, true);
//     //     public LayerMaskParameter layerMask = new LayerMaskParameter(0);
//     //
//     //     public bool IsActive() => radius.value > 0;
//     //     public bool IsTileCompatible() => false;
//     // }
//
//     [System.Serializable]
//     public class GaussianBlurRenderPassSettings
//     {
//         public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
//         public LayerMask layerMask;
//         [Range(0, 6)] public float radius;
//         [Range(1, 10)] public int iteration = 5;
//         
//         public bool IsActive() => radius > 0;
//     }
//     
//     public class GaussianBlurRenderPass : ScriptableRenderPass
//     {
//         private const string RenderPassName = "EasyFramework.URP.GaussianBlurRenderPass";
//         private readonly int _temporaryRT = Shader.PropertyToID("EasyFramework.URP.GaussianBlurRenderPassRT");
//         private readonly Material _material;
//
//         private readonly GaussianBlurRenderPassSettings _settings;
//
//         public GaussianBlurRenderPass(GaussianBlurRenderPassSettings settings)
//         {
//             _settings = settings;
//             
//             renderPassEvent = _settings.renderPassEvent;
//             var shader = Application.isPlaying ? F.ResLoader.GetShader("Hidden/EasyFrameworkURP/GaussianBlur") : Shader.Find("Hidden/EasyFrameworkURP/GaussianBlur");
//             _material = CoreUtils.CreateEngineMaterial(shader);
//         }
//
//         public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//         {
//             // Log.Info("Execute");
//             if (_material == null) return;
//             // if (!renderingData.postProcessingEnabled || !renderingData.cameraData.postProcessEnabled) return;
//
//             // GaussianBlur volumeComp = VolumeManager.instance.stack.GetComponent<GaussianBlur>();
//             // if (volumeComp != null && volumeComp.IsActive())
//             // {
//             //     _material.SetFloat("_Radius", volumeComp.radius.value);
//             // }
//             // else 
//             
//             if(!_settings.IsActive()) return;
//             
//             _material.SetFloat("_Radius", _settings.radius);
//
//             // int iteration = (volumeComp?.IsActive() ?? false) ? volumeComp.iteration.value : _settings.iteration;
//             // LayerMask layerMask = (volumeComp?.IsActive() ?? false) ? volumeComp.layerMask.value : _settings.layerMask;
//             
//             int iteration = _settings.iteration;
//             // LayerMask layerMask = _settings.layerMask;
//
//             CommandBuffer cmd = CommandBufferPool.Get(RenderPassName);
//             
//             // RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
//             // cmd.GetTemporaryRT(_temporaryRT, opaqueDesc);
//             
//             RenderTextureDescriptor tempDescriptor = renderingData.cameraData.cameraTargetDescriptor;   //声明临时RT
//             int rtWidth = tempDescriptor.width;         //定义临时RT的宽度
//             int rtHeight = tempDescriptor.height;       //定义临时RT的高度
//             cmd.GetTemporaryRT(_temporaryRT , rtWidth , rtHeight , depthBuffer:0 , FilterMode.Point , format:RenderTextureFormat.Default);
//
//             var cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
//             for (int i = 0; i < iteration; i++)
//             {
//                 cmd.Blit(cameraColorTargetHandle, _temporaryRT, _material);
//                 cmd.Blit(_temporaryRT, cameraColorTargetHandle);
//             }
//
//             // if (layerMask > 0)
//             // {
//             //     var drawingSettings = CreateDrawingSettings(new ShaderTagId("UniversalForward"), ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
//             //     var filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);
//             //     context.ExecuteCommandBuffer(cmd);
//             //     cmd.Clear();
//             //     context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
//             // }
//
//             context.ExecuteCommandBuffer(cmd);
//             CommandBufferPool.Release(cmd);
//         }
//
//         // Cleanup any allocated resources that were created during the execution of this render pass.
//         public override void OnCameraCleanup(CommandBuffer cmd)
//         {
//             base.OnCameraCleanup(cmd);
//             
//             cmd.ReleaseTemporaryRT(_temporaryRT);
//         }
//     }
// }
//
