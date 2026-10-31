using EasyFramework;
using UnityEngine;

namespace EasyFramework.Samples.Basics
{
    // Prepare Cube.prefab in an AssetBundleBuilder build directory.
    public sealed class ResourceExample : MonoBehaviour, IResRequest
    {
        public bool Alive => !_destroyed;
        private GameObject _cube;
        private bool _destroyed;
        private bool _loading;
        private bool _requested;

        private void Start() => LoadAsync().Forget();

        private async ETask LoadAsync()
        {
            await F.InitializeAsync();
            if (_destroyed) return;

            _loading = true;
            _requested = true;
            try
            {
                var instance = await F.ResLoader.CreateObjAsync("Cube", request: this);
                if (_destroyed)
                {
                    if (instance != null) Destroy(instance);
                }
                else
                {
                    _cube = instance;
                }
            }
            finally
            {
                _loading = false;
                if (_destroyed) Release();
            }
        }

        private void OnDestroy()
        {
            _destroyed = true;
            if (!_loading) Release();
        }

        private void Release()
        {
            if (_cube != null) Destroy(_cube);
            _cube = null;
            if (!_requested) return;
            _requested = false;
            F.ResLoader.Unload("Cube", this);
        }
    }
}
