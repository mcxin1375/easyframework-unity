/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/



using UnityEngine;

namespace EasyFramework
{
    public static partial class UnityHelper
    {
        
        // public static T GetComponentInParentEx<T>(this GameObject gameObject) where T : UnityEngine.Component
        // {
        //     if (gameObject == null) return null;
        //
        //     gameObject.GetComponentsInChildren<IWindowComponent>();
        //     
        //     Transform parent = gameObject.transform.parent;
        //     if (parent != null)
        //     {
        //         T t = parent.GetComponent<T>();
        //         if (t != null)
        //             return t;
        //         return parent.gameObject.GetComponentInParentEx<T>();
        //     }
        //
        //     return null;
        // }


        public static void PlayEffect(this GameObject gameObject)
        {
            ParticleSystem[] particles = gameObject.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particle in particles) particle.Play();
        }
        
        public static void PlayEffect(this GameObject gameObject, Vector3 position, Quaternion rotation)
        {
            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;
            
            ParticleSystem[] particles = gameObject.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particle in particles) particle.Play();
        }
    }
}
