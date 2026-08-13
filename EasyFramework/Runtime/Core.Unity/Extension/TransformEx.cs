/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    public static class TransformEx
    {
        /// <summary>
        /// 设置localScale
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="val"></param>
        public static void SetScaleEx(this Transform transform, float val)
        {
            transform.localScale = Vector3.one * val;
        }

        /// <summary>
        /// 重设 localPosition localRotation localScale
        /// </summary>
        /// <param name="transform"></param>
        public static void ResetLocalPropertyEx(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        public static Transform FindChildEx(this Transform transform, string childName, bool includeInactive = true)
        {
            if (transform.name == childName) return transform;

            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (!includeInactive && !child.gameObject.activeSelf) continue;
                
                if (child.name == childName) return child;

                Transform result = FindChildEx(child, childName, includeInactive);
                if (result != null) return result;
            }
            return null;
        }

        /// <summary>
        /// 查找父对象
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="targetName"></param>
        /// <returns></returns>
        public static Transform FindParentEx(this Transform transform, string targetName)
        {
            while (transform.parent != null)
            {
                transform = transform.parent;
                if (transform.name == targetName) return transform;
            }
            return null;
        }
        
        public static bool IsParentNameOf(this Transform transform, string targetName)
        {
            if (transform.name == targetName) return true;
            while (transform.parent != null)
            {
                transform = transform.parent;
                if (transform.name == targetName) return true;
            }
            return false;
        }

    }
}
