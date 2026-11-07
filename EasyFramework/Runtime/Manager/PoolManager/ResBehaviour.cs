/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/24
// describe:单个ab资源的对象池
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    public class ResBehaviour : MonoBehaviour
    {
        internal ResPoolBehaviour PoolBehaviour;
    }
    
    public static class ResBehaviourEx
    {
        public static void TryReturnPoolEx(this GameObject go)
        { 
            var behaviour = go.GetComponent<ResBehaviour>();
            if (behaviour != null && behaviour.PoolBehaviour != null)
            {
                go.transform.SetParent(behaviour.PoolBehaviour.transform);
                go.SetActive(false);
            }
            else
            {
                Object.Destroy(go);
            }
        }
    }
}