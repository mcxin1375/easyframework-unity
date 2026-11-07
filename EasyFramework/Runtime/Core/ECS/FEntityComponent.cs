/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe: 
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public abstract class FEntityComponent : IEntityComponent
    {
        void IEntityComponent.OnAddComponent() => OnAddComponent();
        void IEntityComponent.OnRemoveComponent() => OnRemoveComponent();
        protected virtual void OnAddComponent() { }
        protected virtual void OnRemoveComponent() { }
    }
}