using EasyFramework;
using UnityEngine;
using UnityEngine.UI;

namespace EasyFramework.Samples.Basics
{
    public sealed class WindowExample : MonoBehaviour
    {
        private ExampleWindow _window;
        private bool _destroyed;

        private void Start() => OpenAsync().Forget();

        private async ETask OpenAsync()
        {
            await F.InitializeAsync();
            if (_destroyed) return;

            var window = await F.WindowManager.OpenAsync<ExampleWindow>(UILayer.HUD);
            if (_destroyed)
            {
                Release(window);
                return;
            }
            _window = window;
        }

        private void OnDestroy()
        {
            _destroyed = true;
            if (_window != null) Release(_window);
            _window = null;
        }

        private static void Release(ExampleWindow window)
        {
            F.WindowManager.Close(window);
            F.WindowManager.Destroy(window);
        }
    }

    // This demo creates its own panel. A normal Window loads a prefab by type name.
    public sealed class ExampleWindow : Window
    {
        protected override GameObject CreateWindowObject(Transform parent)
        {
            var panel = new GameObject("ExampleWindow", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.15f, 0.55f, 0.4f, 0.4f);
            return panel;
        }

        protected override ETask<GameObject> CreateWindowObjectAsync(Transform parent)
            => ETask.FromResult(CreateWindowObject(parent));

        protected override void OnOpen() => Debug.Log("ExampleWindow opened");
        protected override void OnClose() => Debug.Log("ExampleWindow closed");
    }
}
