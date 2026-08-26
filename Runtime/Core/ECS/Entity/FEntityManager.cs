/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/4/25
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace EasyFramework
{
    public class FEntityManager
    {
        // public bool EntityDebugEnabled { get; set; } = Application.isEditor;
        public IReadOnlyCollection<FEntity> Entities => _entities;
        private readonly List<FEntity> _entities = new(128);
        private readonly Queue<FEntity> _entityQueue = new();
        
        internal void Update()
        {
            if (_entityQueue.Count > 0)
            {
                while (_entityQueue.Count > 0)
                {
                    var entity = _entityQueue.Dequeue();
                    if (entity.Alive) _entities.Add(entity);
                }
            }

            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                var entity = _entities[i];
                if (!entity.Alive)
                {
                    _entities.RemoveAt(i);
                    continue;
                }

                entity.Update();
            }
        }
        internal void LateUpdate()
        {
            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                var entity = _entities[i];
                if (!entity.Alive)
                {
                    _entities.RemoveAt(i);
                    continue;
                }
                
                entity.LateUpdate();
            }
        }
        internal void Destroy()
        {
            var arr = _entities.ToArray();
            foreach (var entity in arr)
            {
                entity.Destroy();
                _entities.Remove(entity);
            }
        }
        
        public FEntity CreateEntity()
        {
            var entity = new FEntity(Guid.NewGuid());
            _entityQueue.Enqueue(entity);
            
            // if (EntityDebugEnabled)
            // {
            //     entity.AddComponent<FEntityDebugComponent>();
            // }
            return entity;
        }
        public void DestroyEntity(FEntity entity)
        {
            entity.Destroy();
            if (_entities.Contains(entity)) _entities.Remove(entity);
        }
    }
}