using NUnit.Framework;

namespace EasyFramework.Tests
{
    public class EcsLifecycleTests
    {
        private FWorld _world;

        [SetUp]
        public void SetUp()
        {
            _world = WorldManager.Instance.CreateWorld();
            WorldManager.Instance.Update();
        }

        [TearDown]
        public void TearDown() => _world.Destroy();

        [Test]
        public void DestroyQueuedEntityDoesNotActivate()
        {
            var entity = _world.EntityManager.Create();
            _world.EntityManager.Destroy(entity);
            _world.Update();
            Assert.That(_world.EntityManager.Entities.Count, Is.EqualTo(0));
            Assert.That(entity.Alive, Is.False);
        }

        [Test]
        public void ComponentCallbacksRunOnceAndDuplicateTypesAreRejected()
        {
            var entity = _world.EntityManager.Create();
            var component = entity.AddComponent<TrackingComponent>();
            Assert.That(component.AddCount, Is.EqualTo(1));
            Assert.Throws<System.InvalidOperationException>(() => entity.AddComponent<TrackingComponent>());

            entity.RemoveComponent<TrackingComponent>();
            entity.RemoveComponent<TrackingComponent>();
            Assert.That(component.RemoveCount, Is.EqualTo(1));
        }

        [Test]
        public void UpdateAndLateUpdateRunOnce()
        {
            var system = _world.CreateSystem<TrackingSystem>();
            _world.Update();
            _world.LateUpdate();
            Assert.That(system.UpdateCount, Is.EqualTo(1));
            Assert.That(system.LateUpdateCount, Is.EqualTo(1));
        }

        [Test]
        public void DestroyIsTerminal()
        {
            var system = _world.CreateSystem<TrackingSystem>();
            _world.Update();
            _world.Destroy();
            _world.Destroy();
            _world.Update();
            _world.LateUpdate();
            Assert.That(system.DestroyCount, Is.EqualTo(1));
            Assert.That(system.UpdateCount, Is.EqualTo(1));
            Assert.That(_world.SystemList, Is.Empty);
        }

        private sealed class TrackingComponent : IEntityComponent
        {
            public int AddCount;
            public int RemoveCount;
            public void OnAddComponent() => AddCount++;
            public void OnRemoveComponent() => RemoveCount++;
        }

        public sealed class TrackingSystem : FSystem
        {
            public int UpdateCount;
            public int LateUpdateCount;
            public int DestroyCount;
            protected override void OnUpdate() => UpdateCount++;
            protected override void OnLateUpdate() => LateUpdateCount++;
            protected override void OnDestroy() => DestroyCount++;
        }
    }
}
