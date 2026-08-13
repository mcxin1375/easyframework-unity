
namespace EasyFramework
{
    public interface IEntityComponent<T1>
    {
        T1 TValue1 { get; }
        internal void SetParams(T1 p);
    }
    public interface IEntityComponent<T1, T2>
    {
        T1 TValue1 { get; }
        T2 TValue2 { get; }
        internal void SetParams(T1 t1, T2 t2);
    }
    public interface IEntityComponent<T1, T2, T3>
    {
        T1 TValue1 { get; }
        T2 TValue2 { get; }
        T3 TValue3 { get; }
        internal void SetParams(T1 t1, T2 t2, T3 t3);
    }
    public abstract class FEntityComponent<T1> : FEntityComponent, IEntityComponent<T1>
    {
        public T1 TValue1 { get; protected set; }
        void IEntityComponent<T1>.SetParams(T1 p)
        {
            TValue1 = p;
        }
    }
    public abstract class FEntityComponent<T1, T2> : FEntityComponent, IEntityComponent<T1, T2>
    {
        public T1 TValue1 { get; protected set; }
        public T2 TValue2 { get; protected set; }
        void IEntityComponent<T1, T2>.SetParams(T1 t1, T2 t2)
        {
            TValue1 = t1;
            TValue2 = t2;
        }
    }
    public abstract class FEntityComponent<T1, T2, T3> : FEntityComponent, IEntityComponent<T1, T2, T3>
    {
        public T1 TValue1 { get; protected set; }
        public T2 TValue2 { get; protected set; }
        public T3 TValue3 { get; protected set; }
        void IEntityComponent<T1, T2, T3>.SetParams(T1 t1, T2 t2, T3 t3)
        {
            TValue1 = t1;
            TValue2 = t2;
            TValue3 = t3;
        }
    }

    public abstract class FEntityComponent : IEntityComponent
    {
        public FEntity FEntity { get; private set; }
        void IEntityComponent.Create(FEntity entity)
        {
            FEntity = entity;
            OnCreate();
        }
        void IEntityComponent.Destroy()
        {
            FEntity = null;
            OnDestroy();
        }
        void IEntityComponent.Update() => OnUpdate();
        void IEntityComponent.LateUpdate() => OnLateUpdate();
        protected virtual void OnCreate() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
    }
}