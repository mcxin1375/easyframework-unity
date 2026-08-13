/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;
using UnityEngine.UI;

namespace EasyFramework
{
    public static partial class UnityHelper
    {
        
        public static Vector3 ScreenPointToUGUIPoint(this Canvas canvas, Vector3 screenPosition)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out var localPoint);
                return canvasRect.TransformPoint(new Vector3(localPoint.x, localPoint.y, 0f));
            }

            Camera worldCamera = canvas.worldCamera;
            if (worldCamera == null)
            {
                Debug.LogWarning($"[{nameof(UnityHelper)}] Canvas '{canvas.name}' has no worldCamera assigned.", canvas);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out var localPoint);
                return canvasRect.TransformPoint(new Vector3(localPoint.x, localPoint.y, 0f));
            }

            if (worldCamera.orthographic)
            {
                Vector3 worldPos = worldCamera.ScreenToWorldPoint(screenPosition);
                return new Vector3(worldPos.x, worldPos.y, canvasRect.position.z);
            }

            screenPosition.z = canvas.planeDistance;
            return worldCamera.ScreenToWorldPoint(screenPosition);
        }
    }
}
