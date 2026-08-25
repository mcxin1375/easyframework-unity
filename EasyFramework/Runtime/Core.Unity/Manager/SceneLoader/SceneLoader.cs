using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EasyFramework
{
    internal class SceneLoader : Singleton<SceneLoader>, ISceneLoader
    {
        public string CurrentActiveScene { get; private set; }

        public IReadOnlyDictionary<string, SceneInfo> SceneDict => _sceneDict;
        private readonly Dictionary<string, SceneInfo> _sceneDict = new();

        public SceneLoader()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, bool activateOnLoad = true)
        {
            if (!_sceneDict.TryGetValue(sceneName, out var info))
            {
                info = new SceneInfo(sceneName);
                _sceneDict.Add(sceneName, info);
            }
            
            info.Load(mode);
            if (activateOnLoad) info.TrySetActive();
        }
        public async ETask LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, bool activateOnLoad = true)
        {
            if (!_sceneDict.TryGetValue(sceneName, out var info))
            {
                info = new SceneInfo(sceneName);
                _sceneDict.Add(sceneName, info);
            }

            await info.LoadAsync(mode);
            if (activateOnLoad) info.TrySetActive();
        }
        public ETask UnloadSceneAsync(string sceneName)
        {
            if (_sceneDict.TryGetValue(sceneName, out var info)) return info.UnloadAsync();
            return ETask.CompletedTask;
        }
        public async ETask UnloadAllSceneAsync()
        {
            foreach (var info in _sceneDict.Values) await info.UnloadAsync();
        }
        public float GetLoadProgress(string sceneName)
        {
            if (_sceneDict.TryGetValue(sceneName, out var info)) return info.LoadingProgress;
            return 0;
        }
        public float GetUnloadProgress(string sceneName)
        {
            if (_sceneDict.TryGetValue(sceneName, out var info)) return info.UnloadingProgress;
            return 0;
        }
        public bool IsLoaded(string sceneName)
        {
            if (_sceneDict.TryGetValue(sceneName, out var info)) return info.State == EResState.Loaded;
            return false;
        }
        public bool IsLoading(string sceneName)
        {
            if (_sceneDict.TryGetValue(sceneName, out var info)) return info.State == EResState.Loading;
            return false;
        }
        public bool IsUnloading(string sceneName)
        {
            if (_sceneDict.TryGetValue(sceneName, out var info)) return info.State == EResState.Unloading;
            return false;
        }
        public Scene GetScene(string sceneName)
        {
            if (_sceneDict.TryGetValue(sceneName, out var info)) return info.Scene;
            return default;
        }
        public void SetActive(string sceneName)
        {
            if (_sceneDict.TryGetValue(sceneName, out var info)) info.TrySetActive();
        }
        public GameObject FindActiveSceneRootObj(string name) => FindSceneRootObj(CurrentActiveScene, name);
        public GameObject FindSceneRootObj(string sceneName, string name)
        {
            if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(name)) return null;
            if (!_sceneDict.TryGetValue(sceneName, out var info)) return null;
            return info.FindSceneRootObj(name);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var sceneName = scene.name;
#if UNITY_EDITOR
            sceneName = Path.GetFileName(sceneName);
            
            // Reset assetBundle shader
            if (F.ResLoader is AssetBundleLoader)
            {
                // AssetBundleHelper.ResetShaderEditorOnly(scene);
            }
#endif
            
            if (_sceneDict.TryGetValue(sceneName, out var info)) info.OnLoaded(scene, mode);
        }
        private void OnSceneUnloaded(Scene scene)
        {
            var sceneName = scene.name;
#if UNITY_EDITOR
            sceneName = Path.GetFileName(sceneName);
#endif
            
            if (_sceneDict.TryGetValue(sceneName, out var info)) info.OnUnloaded(scene);
        }
        private void OnActiveSceneChanged(Scene removeScene, Scene addScene)
        {
            var addSceneName = addScene.name;
            var removeSceneName = removeScene.name;
#if UNITY_EDITOR
            addSceneName = Path.GetFileName(addSceneName);
            removeSceneName = Path.GetFileName(removeSceneName);
            
            // Reset assetBundle shader
            if (F.ResLoader is AssetBundleLoader)
            {
                if (RenderSettings.skybox)
                    RenderSettings.skybox.shader = Shader.Find(RenderSettings.skybox.shader.name);
            }
#endif

            if (_sceneDict.TryGetValue(addSceneName, out var addComp))
            {
                CurrentActiveScene = addComp.SceneName;
                addComp.OnActiveChanged(true);
            }
            if (!string.IsNullOrEmpty(removeSceneName) && _sceneDict.TryGetValue(removeSceneName, out var removeComp) && removeScene.IsValid()) removeComp.OnActiveChanged(false);
        }
    }
}