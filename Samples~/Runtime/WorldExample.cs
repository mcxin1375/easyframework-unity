namespace EasyFramework.Samples.Modules
{
    // 当前没有 F.WorldManager，FWorld 构造函数为 internal。
    // 集成层已有 World 时用 world.CreateSystem<MovementSystem>()，并负责 Update/Destroy。
    public sealed class MovementSystem : FSystem
    {
        protected override void OnCreate() => FDebug.Log("MovementSystem 创建");

        protected override void OnUpdate()
        {
            // 更新接入该 World 的业务数据。
        }

        protected override void OnDestroy() => FDebug.Log("MovementSystem 销毁");
    }

    public sealed class PositionComponent : IEntityComponent
    {
        public float X;
        public void OnAddComponent() => X = 0;
        public void OnRemoveComponent() { }
    }

    public static class WorldExample
    {
        // 这里只接收集成层提供的有效 World，不绕过 internal 构造入口。
        public static void CreateSystems(FWorld world)
        {
            world.CreateSystem<MovementSystem>();
            world.CreateSystems(typeof(MovementSystem).Assembly);
            FDebug.Log($"World {world.Index}, Alive={world.Alive}, Systems={world.SystemList.Count}");
        }

        public static void UseEntity(FWorld world)
        {
            var entity = world.EntityManager.Create();
            entity.AddComponent<PositionComponent>();
            bool hasPosition = entity.HasComponent<PositionComponent>();
            PositionComponent position = entity.GetComponent<PositionComponent>();
            IEntityComponent[] components = entity.GetComponents();
            if (hasPosition) position.X = 1;
            entity.RemoveComponent<PositionComponent>();
            entity.AddComponent(new PositionComponent());
            FDebug.Log($"Entity={entity.Guid}, Alive={entity.Alive}, Components={components.Length}");
            world.EntityManager.Destroy(entity);
        }

        // 集成层统一调用；Destroy 为终态，不能对已销毁 World 继续创建实体。
        public static void Update(FWorld world) => world.Update();
        public static void Destroy(FWorld world) => world.Destroy();
    }
}
