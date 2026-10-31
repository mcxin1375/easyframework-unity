/* 
author: Cookie(mcx)
date: 2024/4/8
describe:
*/

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EasyFramework.Profiler
{
    public class MultipleCameraDebugBehaviour : MonoBehaviour
    {
        private readonly List<Camera> _cameras = new List<Camera>();
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        
        void LateUpdate()
        {
            Camera[] arr = GameObject.FindObjectsOfType<Camera>();
            if (arr.Length > 1)
            {
                _cameras.Clear();
                foreach (Camera camera in arr)
                {
                    if (!camera.gameObject.activeInHierarchy || !camera.enabled) continue;
                    var v = camera.GetUniversalAdditionalCameraData();
                    if (v.renderType != CameraRenderType.Base) continue;
                    if (camera.targetTexture != null) continue;

                    _cameras.Add(camera);
                }

                if (_cameras.Count > 1)
                {
                    _stringBuilder.Clear();
                    _stringBuilder.Append("------------------------ MultipleCameraDebugEx \n");
                    foreach (Camera camera in _cameras)
                    {
                        _stringBuilder.Append($"-- camera: {camera.name} \n");
                    }

                    Debug.LogError(_stringBuilder.ToString());
                }
            }
        }
    }
}