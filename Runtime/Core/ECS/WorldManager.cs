/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe: 
//----------------------------------------------------------------*/

using System.Collections.Generic;

namespace EasyFramework
{
    public class WorldManager : Singleton<WorldManager>
    {
        private FWorld _mainWorld;
        public FWorld MainWorld
        {
            get
            {
                if (_mainWorld == null) _mainWorld = CreateWorld();
                return _mainWorld;
            }
        }

        public IReadOnlyList<FWorld> WorldList => _worldList;
        private readonly List<FWorld> _worldList = new();
        private readonly Queue<FWorld> _queue = new();
        
        private int _worldIndex = 0;
        private bool _alive = true;
        
        public void Update()
        {
            if (!_alive) return;
            if (_queue.Count > 0)
            {
                while (_queue.Count > 0) _worldList.Add(_queue.Dequeue());
            }

            for (var i = _worldList.Count - 1; i >= 0; i--)
            {
                if (!_worldList[i].Alive) _worldList.RemoveAt(i);
                else _worldList[i].Update();
            }
        }

        public void LateUpdate()
        {
            if (!_alive) return;
            for (var i = _worldList.Count - 1; i >= 0; i--)
            {
                if (!_worldList[i].Alive) _worldList.RemoveAt(i);
                else _worldList[i].LateUpdate();
            }
        }

        public void Destroy()
        {
            if (!_alive) return;
            _alive = false;
            if (_queue.Count > 0)
            {
                while (_queue.Count > 0) _queue.Dequeue().Destroy();
            }
            for (var i = _worldList.Count - 1; i >= 0; i--) _worldList[i].Destroy();
            _worldList.Clear();
            _mainWorld = null;
        }

        public FWorld CreateWorld()
        {
            if (!_alive) throw new System.InvalidOperationException("Cannot create a world after destruction.");
            var world = new FWorld(_worldIndex++);
            _queue.Enqueue(world);
            return world;
        }
    }
}
