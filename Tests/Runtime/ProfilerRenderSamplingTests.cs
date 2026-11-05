using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EasyFramework.Profiler;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace EasyFramework.Tests
{
    public class ProfilerRenderSamplingTests
    {
        [UnityTest]
        public IEnumerator Update_RenderedGeometry_PublishesRenderCountersAndPanelText() => RenderScene(-1, false);

        [UnityTest]
        public IEnumerator Update_LimitedFrameRate_PublishesRenderCountersAndPanelText() => RenderScene(60, false);

        [UnityTest]
        public IEnumerator Update_BuiltInWithTwoCameras_PublishesCumulativeCounters() => RenderScene(60, true);

        private static IEnumerator RenderScene(int frameRate, bool builtIn)
        {
            int oldFrameRate = Application.targetFrameRate;
            var oldPipeline = GraphicsSettings.renderPipelineAsset;
            var oldQualityPipeline = QualitySettings.renderPipeline;
            var cameraObject = new GameObject("Profiler render regression camera", typeof(Camera));
            var secondCameraObject = new GameObject("Profiler second camera", typeof(Camera));
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var panelObject = new GameObject("Profiler render regression panel");
            var target = new RenderTexture(128, 128, 24);
            var secondTarget = new RenderTexture(128, 128, 24);
            Material material = null;
            int renderedFrames = 0;
            var camera = cameraObject.GetComponent<Camera>();
            var secondCamera = secondCameraObject.GetComponent<Camera>();
            void OnRendered(ScriptableRenderContext context, Camera renderedCamera)
            {
                if (renderedCamera == camera) renderedFrames++;
            }
            void OnPostRendered(Camera renderedCamera)
            {
                if (GraphicsSettings.currentRenderPipeline == null && renderedCamera == camera) renderedFrames++;
            }
            RenderPipelineManager.endCameraRendering += OnRendered;
            Camera.onPostRender += OnPostRendered;
            try
            {
                Application.targetFrameRate = frameRate;
                if (builtIn)
                {
                    GraphicsSettings.renderPipelineAsset = null;
                    QualitySettings.renderPipeline = null;
                }
                camera.transform.position = new Vector3(0, 0, -3);
                camera.orthographic = true;
                camera.orthographicSize = 1;
                camera.cullingMask = 1 << 31;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.targetTexture = target;
                secondCamera.CopyFrom(camera);
                secondCamera.transform.position = camera.transform.position;
                secondCamera.depth = camera.depth + 1;
                secondCamera.targetTexture = secondTarget;
                cube.layer = 31;
                cube.transform.position = Vector3.zero;
                var shader = Shader.Find(builtIn ? "Unlit/Color" : "Universal Render Pipeline/Unlit");
                if (shader == null) shader = Shader.Find("Unlit/Color");
                Assert.That(shader, Is.Not.Null);
                material = new Material(shader) { color = Color.white };
                cube.GetComponent<Renderer>().sharedMaterial = material;
                var panel = panelObject.AddComponent<ProfilerGUIBehaviour>();
                var stats = panel.ProfilerRecorderEx;
                var recorders = (Dictionary<string, ProfilerRecorder>)typeof(ProfilerRecorderEx)
                    .GetField("_profilerRecorderDict", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(stats);
                int zeroSnapshots = 0;
                for (int frame = 0; frame < 90; frame++)
                {
                    yield return null;
                    if (frame >= 45 && (stats.BatchesCount == 0 || stats.DrawCallsCount == 0 ||
                        stats.TrianglesCount == 0 || stats.VerticesCount == 0 || stats.SetPassCallsCount == 0))
                        zeroSnapshots++;
                }
                Assert.That(target.IsCreated(), Is.True);
                Assert.That(secondTarget.IsCreated(), Is.True);
                Assert.That(renderedFrames, Is.GreaterThan(0), "The test camera must actually render.");
                var draw = recorders[nameof(ProfilerRecorderEx.DrawCallsCount)];
                string diagnostics = $"targetFrameRate={frameRate}; builtIn={builtIn}; renderedFrames={renderedFrames}; " +
                    $"draw Last={draw.LastValue}, Current={draw.CurrentValue}, Count={draw.Count}; " +
                    $"B/DC/T/V/SC={stats.BatchesCount}/{stats.DrawCallsCount}/{stats.TrianglesCount}/{stats.VerticesCount}/{stats.SetPassCallsCount}";
                TestContext.WriteLine(diagnostics);
                Assert.That(zeroSnapshots, Is.Zero, diagnostics);
                Assert.That(stats.DrawCallsCount, Is.GreaterThan(0), diagnostics);
                var header = (string)typeof(ProfilerGUIBehaviour)
                    .GetField("_headerStr", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(panel);
                StringAssert.Contains($" DC: {stats.FormatNumber(nameof(ProfilerRecorderEx.DrawCallsCount))}", header);
            }
            finally
            {
                RenderPipelineManager.endCameraRendering -= OnRendered;
                Camera.onPostRender -= OnPostRendered;
                Application.targetFrameRate = oldFrameRate;
                Object.DestroyImmediate(panelObject);
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(secondCameraObject);
                Object.DestroyImmediate(cube);
                if (material != null) Object.DestroyImmediate(material);
                target.Release();
                secondTarget.Release();
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(secondTarget);
                GraphicsSettings.renderPipelineAsset = oldPipeline;
                QualitySettings.renderPipeline = oldQualityPipeline;
            }
        }
    }
}
