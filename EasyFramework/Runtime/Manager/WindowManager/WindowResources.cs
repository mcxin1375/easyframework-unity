/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    public abstract class WindowResources : Window
    {
        protected virtual string ResourcePath => GetType().Name;
        
        protected sealed override GameObject CreateWindowObject(Transform parent)
        {
            return F.ResLoader.CreateObjResources(ResourcePath, parent);
        }

        protected sealed override ETask<GameObject> CreateWindowObjectAsync(Transform parent)
        {
            return ETask.FromResult(F.ResLoader.CreateObjResources(ResourcePath, parent));
        }
    }
}