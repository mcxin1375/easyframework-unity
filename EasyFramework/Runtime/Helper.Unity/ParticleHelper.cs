/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    public static class ParticleHelper
    {
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
