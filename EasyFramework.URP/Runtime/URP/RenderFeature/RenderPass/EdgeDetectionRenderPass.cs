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
// using UnityEngine.Rendering.RenderGraphModule;
// using UnityEngine.Rendering.Universal;
//
// namespace EasyFramework.URP
// {
//     // [Serializable, VolumeComponentMenu("EasyFrameworkURP/EdgeDetection")]
//     // public class EdgeDetection : VolumeComponent, IPostProcessComponent
//     // {
//     //     public ClampedFloatParameter edgeWidth = new ClampedFloatParameter(0 , 0 , 3);
//     //     public ClampedFloatParameter edgeForce = new ClampedFloatParameter(1 , 1 , 5);
//     //     public ClampedFloatParameter edgeOnly = new ClampedFloatParameter(0 , 0 , 1);
//     //     public ColorParameter edgeColor = new ColorParameter(Color.black , true);
//     //     public ColorParameter backGroundColor = new ColorParameter(Color.white , true);
//     //
//     //     public bool IsActive() => edgeWidth.value > 0;
//     //     public bool IsTileCompatible() => false;
//     // }
//     
//     [System.Serializable]
//     public class EdgeDetectionRenderPassSettings
//     {
//         public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
//
//         [Range(0, 3)] public float edgeWidth;
//         [Range(1, 5)] public float edgeForce = 1;
//         [Range(0, 1)] public float edgeOnly;
//         public Color edgeColor = Color.black;
//         public Color backGroundColor = Color.white;
//
//         public bool IsActive() => edgeWidth > 0;
//     }
//     
//     public class EdgeDetectionRenderPass : ScriptableRenderPass
//     {
//         private const string RenderPassName = "EdgeDetectionRenderPass";
//         private readonly int _temporaryRT = Shader.PropertyToID("EdgeDetectionRenderPassRT");
//         private readonly Material _material;
//
//         private readonly EdgeDetectionRenderPassSettings _settings;
//         
//         public EdgeDetectionRenderPass(EdgeDetectionRenderPassSettings settings)
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
//             if (_material == null) return;
//             if (!renderingData.postProcessingEnabled) return;
//
//             // EdgeDetection volumeComp = VolumeManager.instance.stack.GetComponent<EdgeDetection>();
//             // if (volumeComp != null && volumeComp.IsActive())
//             // {
//             //     _material.SetFloat("_EdgeWidth", volumeComp.edgeWidth.value);
//             //     _material.SetFloat("_EdgeForce", volumeComp.edgeForce.value);
//             //     _material.SetFloat("_EdgeOnly", volumeComp.edgeOnly.value);
//             //     _material.SetColor("_EdgeColor", volumeComp.edgeColor.value);
//             //     _material.SetColor("_BackgroundColor", volumeComp.backGroundColor.value);
//             // }
//             // else 
//             if(_settings.IsActive())
//             {
//                 _material.SetFloat("_EdgeWidth", _settings.edgeWidth);
//                 _material.SetFloat("_EdgeForce", _settings.edgeForce);
//                 _material.SetFloat("_EdgeOnly", _settings.edgeOnly);
//                 _material.SetColor("_EdgeColor", _settings.edgeColor);
//                 _material.SetColor("_BackgroundColor", _settings.backGroundColor);
//             }
//             else return;
//
//             CommandBuffer cmd = CommandBufferPool.Get(RenderPassName);
//
//             RenderTextureDescriptor tempDescriptor = renderingData.cameraData.cameraTargetDescriptor;   //声明临时RT
//             int rtWidth = tempDescriptor.width;         //定义临时RT的宽度
//             int rtHeight = tempDescriptor.height;       //定义临时RT的高度
//             cmd.GetTemporaryRT(_temporaryRT , rtWidth , rtHeight , depthBuffer:0 , FilterMode.Point , format:RenderTextureFormat.Default);
//         
//             var source = renderingData.cameraData.renderer.cameraColorTargetHandle;       //当前相机
//             cmd.Blit(source , _temporaryRT , _material);
//             cmd.Blit(_temporaryRT , source);
//
//             context.ExecuteCommandBuffer(cmd);
//             CommandBufferPool.Release(cmd);
//         }
//
//         public override void OnCameraCleanup(CommandBuffer cmd)
//         {
//             base.OnCameraCleanup(cmd);
//             
//             cmd.ReleaseTemporaryRT(_temporaryRT);
//         }
//     }
// }