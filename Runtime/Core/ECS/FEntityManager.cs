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
        public IReadOnlyCollection<FEntity> Entities => _entities;
        private readonly List<FEntity> _entities = new(128);
        private readonly Queue<FEntity> _tmpQueue = new();
        private bool _alive = true;

        internal FEntityManager()
        {
            
        }

        internal void Destroy()
        {
            if (!_alive) return;
            _alive = false;
            for (var i = _entities.Count - 1; i >= 0; i--)
            {
                _entities[i].Destroy();
            }
            _entities.Clear();
            while (_tmpQueue.Count > 0) _tmpQueue.Dequeue().Destroy();
        }
        
        public FEntity Create()
        {
            if (!_alive) throw new InvalidOperationException("Cannot create an entity in a destroyed manager.");
            var entity = new FEntity(Guid.NewGuid());
            _tmpQueue.Enqueue(entity);
            return entity;
        }
        public void Destroy(FEntity entity)
        {
            if (entity == null) return;
            entity.Destroy();
            _entities.Remove(entity);
        }
    }
}
