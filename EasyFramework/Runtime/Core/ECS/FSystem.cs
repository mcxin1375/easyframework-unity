
using System;
using System.Reflection;

namespace EasyFramework
{
    public abstract class FSystem : ISystem
    {
        public int Order { get; }
        protected FSystem()
        {
            var attribute = GetType().GetCustomAttribute<FSystemOrderAttribute>();
            Order = attribute?.Order ?? 0;
        }
        void ISystem.Create()
        {
            // FDebug.Log($"Create FSystem [{GetType().Name}]", LogType.EasyFramework);
            
            var time = FDebug.StartTime($"{GetType().Name}.Create");
            OnCreate();
            time.StopAndPrint();
        }
        void ISystem.Destroy()
        {
            // FDebug.Log($"Destroy FSystem [{GetType().Name}]", LogType.EasyFramework);
            var time = FDebug.StartTime($"{GetType().Name}.Destroy");
            OnDestroy();
            time.StopAndPrint();
        }
        void ISystem.Update() => OnUpdate();
        void ISystem.LateUpdate() => OnLateUpdate();
        protected virtual void OnCreate() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
    }
    
    public interface ISystem
    {
        int Order { get; }
        void Create();
        void Destroy();
        void Update();
        void LateUpdate();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class FSystemOrderAttribute : Attribute
    {
        public readonly int Order;
        public FSystemOrderAttribute(int order)
        {
            Order = order;
        }
    }

}