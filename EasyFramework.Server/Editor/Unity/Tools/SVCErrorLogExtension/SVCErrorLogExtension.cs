// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2023/6/23
// // describe:
// //----------------------------------------------------------------*/
//
// using System.Linq;
// using System.Threading.Tasks;
// using EasyFramework.Editor;
// using EasyFramework.Profiler;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.Rendering;
//
// namespace EasyFramework.Server.Editor
// {
//     public class SVCErrorLogExtension : ISVCCollectorExtension
//     {
//         public void OnExecuteBefore()
//         {
//             _ = UpdateFromServerAsync();
//         }
//
//         public static void ClearSVCErrorConfig()
//         {
//             _ = ServerEditorAPI.ClearSVCErrorConfigAsync();
//         }
//
//         public static async Task UpdateFromServerAsync()
//         {
//             var settings = SVCErrorLogCollectorSettings.Instance;
//             
//             var config = await ServerEditorAPI.GetSVCErrorConfigAsync();
//             var svc = new ShaderVariantCollection();
//             foreach (var errorLog in config.ErrorLogs)
//             {
//                 if (!ShaderVariantInfo.TryParseFromLog(errorLog, out var info)) continue;
//                 
//                 var shader = Shader.Find(info.ShaderName);
//                 if (!shader || shader.passCount <= info.Pass)
//                 {
//                     // Debug.LogWarning($"shader: {shader.name}, passCount: {shader.passCount}");
//                     continue;
//                 }
//
//                 var shaderTagId = shader.FindPassTagValue(info.Pass, new ShaderTagId("LightMode"));
//                 var tagToPass = settings.tagToPass?.FirstOrDefault(v => v.shaderTagName == shaderTagId.name);
//                 if (tagToPass == null)
//                 {
//                     // Debug.LogWarning($"shader: {shader.name}, lightMode: {shaderTagId.name}. tagToPass can not found.");
//                     continue;
//                 }
//                 // Debug.Log($"shader: {shader.name}, lightMode: {shaderTagId.name}, passType: {tagToPass.passType}");
//
//                 var variant = new ShaderVariantCollection.ShaderVariant();
//                 variant.shader = shader;
//                 variant.keywords = info.Keywords;
//                 variant.passType = tagToPass.passType;
//                 
//                 if (!svc.Contains(variant)) svc.Add(variant);
//             }
//             svc.name = $"{SVCCollectorSettings.Instance.SvcFileName}_ErrorLog";
//             
//             FileHelper.CreateDirectory(SVCCollectorSettings.Instance.svcSaveDirectory);
//             var saveFile = $"{SVCCollectorSettings.Instance.svcSaveDirectory}/{svc.name}.shadervariants";
//             
//             AssetDatabase.CreateAsset(svc, saveFile);
//             AssetDatabase.Refresh();
//             
//             if (settings.svnCommitEnabled)
//             {
//                 var arr = new string[]
//                 {
//                     saveFile,
//                     $"{saveFile}.meta"
//                 };
//                 SVNCommand.CommitAll(arr, "", (str) =>
//                 {
//                     Debug.Log(str);
//                 });
//             }
//         }
//
//         // private static PassType ShaderTagIdToPassType(ShaderTagId shaderTagId)
//         // {
//         //     return shaderTagId.name switch
//         //     {
//         //         "ALWAYS" => PassType.Normal,
//         //         "FORWARDADD" => PassType.ForwardAdd,
//         //         "FORWARDBASE" => PassType.ForwardBase,
//         //         "DEFERRED" => PassType.Deferred,
//         //         "VERTEX" => PassType.Vertex,
//         //         "VERTEXLM" => PassType.VertexLM,
//         //         "SHADOWCASTER" => PassType.ShadowCaster,
//         //         "META" => PassType.Meta,
//         //         "MOTIONVECTORS" => PassType.MotionVectors,
//         //         "UNIVERSALGBUFFER" => PassType.ShadowCaster,
//         //         "UniversalForward" => PassType.ScriptableRenderPipeline,
//         //         _ => PassType.Normal,
//         //     };
//         // }
//     }
// }