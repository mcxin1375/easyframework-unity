/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EasyFramework.URP
{
    public class UIRootBehaviourURP : UIBaseBehaviour
    {
        private void Awake()
        {
            var uiRootBehaviour = GetComponent<UIRootBehaviour>();
            if (uiRootBehaviour == null) return;

            switch (uiRootBehaviour.UIRenderMode)
            {
                case EUIRenderMode.Overlay:
                    break;
                case EUIRenderMode.UICamera:
                    
                    if (Camera.main != null)
                    {
                        var uiCamera = uiRootBehaviour.UICamera;
                        var cameraData = uiCamera.GetUniversalAdditionalCameraData();
                        cameraData.renderType = CameraRenderType.Overlay;
            
                        var mainData = Camera.main.GetUniversalAdditionalCameraData();
                        mainData.cameraStack.Add(uiCamera);
                    }
                    break;
            }
        }
    }
}