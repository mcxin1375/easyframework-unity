/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    public class EasyFrameworkBehaviour : MonoBehaviour
    {
        private static EasyFrameworkBehaviour _instance;
        internal static EasyFrameworkBehaviour Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameObject("[EasyFramework]").AddComponent<EasyFrameworkBehaviour>();
                return _instance;
            }
        }

        void Awake()
        {
            Object.DontDestroyOnLoad(gameObject);
        }
    }
}