/*----------------------------------------------------------------
// author??Cookie(mcx)
// date??2023/12/5
// describe??
//----------------------------------------------------------------*/

using System.Collections.Generic;

namespace EasyFramework
{
    internal class ControllerManager : Singleton<ControllerManager>, IControllerManager, IFrameworkTick
    {
        public IController TopController => _enterList.Count > 0 ? _enterList[^1] : null;
        public IReadOnlyList<IController> EnterList => _enterList;
        
        private readonly List<IController> _enterList = new();
        private readonly List<IController> _updateList = new();
        private bool _refresh;

        protected override void OnCreate()
        {
            FBehaviour.Instance.Add(this);
        }

        public bool HasEnter(IController controller) => _enterList.Contains(controller);
        public bool HasActive(IController controller) => controller != null && _enterList.Count > 0 && _enterList[^1] == controller;
        
        public void Enter(IController controller)
        {
            if (TopController == controller) return;
            if (_enterList.Contains(controller)) _enterList.Remove(controller);
            _enterList.Add(controller);

            for (int i = 0; i < _enterList.Count; i++)
            {
                _enterList[i].OnSetActive(i == _enterList.Count - 1);
            }
        }

        public void Remove(IController controller)
        {
            if (_enterList.Contains(controller)) _enterList.Remove(controller);
            
            for (int i = 0; i < _enterList.Count; i++)
            {
                _enterList[i].OnSetActive(i == _enterList.Count - 1);
            }
        }

        public async ETask ExitAllAsync()
        {
            for (int i = _enterList.Count - 1; i >= 0; i--)
            {
                var ctr = _enterList[i];
                _enterList.RemoveAt(i);
                await ctr.ExitAsync();
            }
        }

        void IFrameworkTick.OnUpdate()
        {
            if (_refresh)
            {
                _refresh = false;
                _updateList.Clear();
                _updateList.AddRange(_enterList);
            }

            foreach (var ctr in _updateList) if (ctr.IsEnter) ctr.OnUpdate();
        }

        void IFrameworkTick.OnLateUpdate()
        {
            foreach (var ctr in _updateList) if (ctr.IsEnter) ctr.OnLateUpdate();
        }

        void IFrameworkTick.OnDestroy()
        {
            
        }
    }
}
