using EasyFramework;
using UnityEngine;

namespace EasyFramework.Samples.Basics
{
    public sealed class ExampleController : Controller<ExampleController>
    {
        protected override void OnEnter() => Debug.Log("ExampleController entered");
        protected override void OnExit() => Debug.Log("ExampleController exited");
    }

    public sealed class ControllerExample : MonoBehaviour
    {
        private bool _destroyed;

        private void Start() => RunAsync().Forget();
        private void OnDestroy() => _destroyed = true;

        private async ETask RunAsync()
        {
            await F.InitializeAsync();
            if (_destroyed) return;

            var controller = ExampleController.Instance;
            try
            {
                await controller.EnterAsync();
                Debug.Log($"Controller active: {F.ControllerManager.HasActive(controller)}");
            }
            finally
            {
                if (controller.IsEnter) await controller.ExitAsync();
                ExampleController.DestroyInstance();
            }
        }
    }
}
