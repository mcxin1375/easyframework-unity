/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/8/12
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EasyFramework
{
    public class ControllerResLoader : ControllerComponent, IControllerLoading
    {
        public bool IsLoading => GetIsLoading();
        public bool IsUnloading => GetIsUnloading();
        public float Progress => GetLoadProgress();

        public string TopSceneName => SceneList.Count > 0 ? SceneList[^1] : string.Empty;
        
        public readonly List<string> SceneList = new();
        public readonly List<string> ResList = new();

        void IControllerLoading.OnStartLoading()
        {
            
        }

        protected override async ETask OnExitAsync()
        {
            await UnloadAllAsync();
        }
        
        public async ETask LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive, bool active = true)
        {
            switch (mode)
            {
                case LoadSceneMode.Single:
                    foreach (string s in SceneList) await F.SceneLoader.UnloadSceneAsync(s);
                    SceneList.Clear();
                    break;
                case LoadSceneMode.Additive:
                    if (SceneList.Contains(sceneName)) SceneList.Remove(sceneName);
                    break;
            }
            SceneList.Add(sceneName);
            await F.SceneLoader.LoadSceneAsync(sceneName, mode, active);
        }
        
        public void LoadRes(params string[] resArray)
        {
            foreach (string abName in resArray)
            {
                if (ResList.Contains(abName)) ResList.Remove(abName);
                ResList.Add(abName);
                F.ResLoader.Load(abName);
            }
        }
        // public async EasyTask LoadResAsync(params string[] resArray)
        // {
        //     foreach (string abName in resArray)
        //     {
        //         if (ResList.Contains(abName)) ResList.Remove(abName);
        //         ResList.Add(abName);
        //         await F.ResLoader.Load(abName);
        //     }
        // }

        public ETask UnloadSceneAsync(string sceneName)
        {
            if (SceneList.Contains(sceneName))
            {
                SceneList.Remove(sceneName);
                return F.SceneLoader.UnloadSceneAsync(sceneName);
            }
            return ETask.CompletedTask;
        }
        public async ETask UnloadAllAsync()
        {
            foreach (string abName in ResList) F.ResLoader.Unload(abName);
            ResList.Clear();
            foreach (string abName in SceneList) await F.SceneLoader.UnloadSceneAsync(abName);
            SceneList.Clear();
        }
        // public async EasyTask UnloadAllSceneAsync()
        // {
        //     foreach (string abName in SceneList) await F.SceneSystem.UnloadSceneAsync(abName);
        //     SceneList.Clear();
        // }
        // public async EasyTask UnloadAllResAsync()
        // {
        //     foreach (string abName in ResList) F.ResLoader.Unload(abName);
        //     ResList.Clear();
        // }
        
        public GameObject FindSceneRootObj(string name)
        {
            for (int i = SceneList.Count - 1; i >= 0; i--)
            {
                var obj = F.SceneLoader.FindSceneRootObj(SceneList[i], name);
                if (obj != null) return obj;
            }
            return null;
        }

        public GameObject FindTopSceneRootObj(string name) => SceneList.Count > 0 ? F.SceneLoader.FindSceneRootObj(SceneList[^1], name) : null;

        private bool GetIsLoading()
        {
            foreach (string s in ResList)
            {
                bool isLoading = F.ResLoader.IsLoading(s);
                if (isLoading) return true;
            }
            foreach (string s in SceneList)
            {
                bool isLoading = !F.SceneLoader.IsLoaded(s);
                if (isLoading) return true;
            }
            return false;
        }
        
        private bool GetIsUnloading()
        {
            foreach (string s in ResList)
            {
                if (F.ResLoader.IsUnloading(s)) return true;
            }
            foreach (string s in SceneList)
            {
                if (F.SceneLoader.IsUnloading(s)) return true;
            }
            return false;
        }
        
        private float GetLoadProgress()
        {
            int len = ResList.Count + SceneList.Count;
            if (len == 0) return 1;
            
            float progress = 0;
            foreach (string s in ResList)
            {
                progress += F.ResLoader.GetLoadingProgress(s);
                // Debug.Log($"{s} : {F.AssetBundleLoader.GetLoadProgress(s, true)}");
            }
            foreach (string s in SceneList)
            {
                progress += F.SceneLoader.GetLoadProgress(s);
                // Debug.Log($"{s} : {F.SceneSystem.GetLoadProgress(s)}");
            }
            // Debug.Log($"Load Progress: {progress} / {len}");
            return progress / len;
        }
    }
}