
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EasyFramework.Profiler
{
    public static class ProfilerHelper
    {
        public static void SceneBatchingDebug()
        {
            GameObject[] arr = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject gameObject in arr)
            {
                SceneBatchingDebug(gameObject);
            }
        }

        public static void SceneBatchingDebug(GameObject go)
        {
            Transform[] arr = go.GetComponentsInChildren<Transform>(true);
            StringBuilder sb = new StringBuilder();
            foreach (Transform transform in arr)
            {
                if (!transform.gameObject.isStatic) continue;
                if (transform.localScale.x < 0 || transform.localScale.y < 0 || transform.localScale.z < 0)
                {
                    // Debug.LogError($"SceneBatchingDebugEx - gameObject: {transform.name}  localScale: {transform.localScale}");
                    if (sb.Length == 0)
                    {
                        sb.Append("--------------------- SceneBatchingDebug \n");
                    }

                    sb.Append($"-- gameObject: {transform.name}  localScale: {transform.localScale} \n");
                    // Vector3 v = transform.localScale;
                    // if (v.x < 0) v.x = -v.x;
                    // if (v.y < 0) v.y = -v.y;
                    // if (v.z < 0) v.z = -v.z;
                    // transform.localScale = v;
                }
            }
            
            if (sb.Length > 0)
            {
                Debug.LogError(sb.ToString());
            }
        }

    }
}