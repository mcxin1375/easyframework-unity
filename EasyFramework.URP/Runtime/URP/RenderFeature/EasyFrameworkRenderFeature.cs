// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2024/5/16
// // describe:
// //----------------------------------------------------------------*/
//
// using UnityEngine;
// using UnityEngine.Rendering.Universal;
//
// namespace EasyFramework.URP
// {
//     public class EasyFrameworkRenderFeature : ScriptableRendererFeature
//     {
//         private static EasyFrameworkRenderFeature _instance;
//         public static EasyFrameworkRenderFeature Instance
//         {
//             get
//             {
//                 if (_instance == null)
//                 {
//                     // Log.Info("EasyRenderFeature CreateInstance");
//                     _instance = ScriptableObject.CreateInstance<EasyFrameworkRenderFeature>();
//                 }
//
//                 return _instance;
//             }
//         }
//
//         public GaussianBlurRenderPassSettings gaussianBlurRenderPassSettings = new GaussianBlurRenderPassSettings();
//         public EdgeDetectionRenderPassSettings edgeDetectionRenderPassSettings = new EdgeDetectionRenderPassSettings();
//         
//         private ScriptableRenderPass[] _renderPasses;
//         
//         /// <inheritdoc/>
//         public override void Create()
//         {
//             if (_instance == null) _instance = this;
//
//             // Log.Info("EasyRenderFeature Create");
//             _renderPasses = new ScriptableRenderPass[]
//             {
//                 new GaussianBlurRenderPass(gaussianBlurRenderPassSettings),
//                 new EdgeDetectionRenderPass(edgeDetectionRenderPassSettings)
//             };
//         }
//
//         // Here you can inject one or multiple render passes in the renderer.
//         // This method is called when setting up the renderer once per-camera.
//         public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//         {
//             // Log.Info("EasyRenderFeature AddRenderPasses");
//             // Configures where the render pass should be injected.
//             foreach (ScriptableRenderPass renderPass in _renderPasses)
//             {
//                 renderer.EnqueuePass(renderPass);
//             }
//         }
//
//         private void OnDestroy()
//         {
//             if (Application.isEditor)
//             {
//                 var arr = UniversalRenderPipeline.asset.GetScriptableRendererDataArrayEx();
//                 foreach (var rendererData in arr)
//                 {
//                     if (rendererData.rendererFeatures.Contains(_instance))
//                     {
//                         rendererData.rendererFeatures.Remove(_instance);
//                     }
//                 }
//             }
//         }
//     }
// }