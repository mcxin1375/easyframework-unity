/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    public static class GameObjectUtility
    {
        /// <summary>
        /// 添加一个组件，会先判断组件是否已经存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T AddComponentEx<T>(this GameObject gameObject) where T : UnityEngine.Component
        {
            T t = gameObject.GetComponent<T>();
            if (t != null)
                return t;
            return gameObject.AddComponent<T>();
        }

        /// <summary>
        /// 添加一个组件，会先判断组件是否已经存在
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Component AddComponentEx(this GameObject gameObject, System.Type type)
        {
            Component comp = gameObject.GetComponent(type);
            if (comp != null)
                return comp;
            return gameObject.AddComponent(type);
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="type"></param>
        public static void RemoveComponentEx(this GameObject gameObject, System.Type type)
        {
            Component comp = gameObject.GetComponent(type);
            if (comp != null)
                GameObject.Destroy(comp);
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        public static void RemoveComponentEx<T>(this GameObject gameObject) where T : UnityEngine.Component
        {
            T t = gameObject.GetComponent<T>();
            if (t != null)
                GameObject.Destroy(t);
        }

        /// <summary>
        /// 查找父节点的组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T GetComponentInParentEx<T>(this GameObject gameObject) where T : UnityEngine.Component
        {
            if (gameObject == null)
                return null;
            Transform parent = gameObject.transform.parent;
            if (parent != null)
            {
                T t = parent.GetComponent<T>();
                if (t != null)
                    return t;
                return parent.gameObject.GetComponentInParentEx<T>();
            }

            return null;
        }

        /// <summary>
        /// 设置层级
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="layer">层级</param>
        /// <param name="activeChild">是否处理子对象</param>
        public static void SetLayerEx(this GameObject gameObject, int layer, bool activeChild = true)
        {
            gameObject.layer = layer;

            if (activeChild)
            {
                Transform[] array = gameObject.GetComponentsInChildren<Transform>(true);
                foreach (Transform transform in array)
                {
                    transform.gameObject.layer = layer;
                }
            }
        }
    }
}
