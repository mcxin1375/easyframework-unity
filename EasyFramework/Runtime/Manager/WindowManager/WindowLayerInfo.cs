/*----------------------------------------------------------------
// author:meng cheng xin
// date:2018/3/1
// describe:UI管理类
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyFramework
{
    public class WindowLayerInfo
    {
        public readonly UILayerBehaviour LayerBehaviour;
        private readonly List<IWindow> _windowList = new();
        
        public IReadOnlyList<IWindow> WindowList => _windowList;
        public IWindow TopWindow => _windowList.Count > 0 ? _windowList[^1] : null;
        private Guid _refreshOrderUid;
        
        public WindowLayerInfo(UILayerBehaviour behaviour)
        {
            LayerBehaviour = behaviour;
        }

        public void Add(IWindow window)
        {
            if (_windowList.Contains(window)) _windowList.Remove(window);

            var attribute = window.GetType().GetCustomAttribute<WindowOpenBeforeAttribute>();
            bool added = false;
            if (attribute != null)
            {
                for (int i = 0; i < _windowList.Count; i++)
                {
                    var v = _windowList[i];
                    if (v.GetType() == attribute.WindowType)
                    {
                        _windowList.Insert(i, window);
                        added = true;
                        break;
                    }
                }
            }
            if (!added) _windowList.Add(window);

            if (_refreshOrderUid != Guid.Empty) RefreshOrder();
        }

        public void Remove(IWindow window)
        {
            if (_windowList.Contains(window))
            {
                _windowList.Remove(window);
                
                // 排序过程中，列表发生会重排，避免排序错误
                // 如果不在排序过程中，不触发重排
                if (_refreshOrderUid != Guid.Empty) RefreshOrder();
            }
        }

        public void RefreshOrder()
        {
            if (LayerBehaviour == null) return;

            var currentUid = Guid.NewGuid();
            _refreshOrderUid = currentUid;

            try
            {
                int sortingOrder = LayerBehaviour.Canvas.sortingOrder;
                for (int i = 0; i < _windowList.Count; i++)
                {
                    var v = _windowList[i];

                    v.RefreshOrder(LayerBehaviour, sortingOrder, i);
                    sortingOrder += LayerBehaviour.OrderInterval;

                    if (currentUid != _refreshOrderUid) return;
                }
            }
            catch (Exception e)
            {
                FDebug.LogException(e);
            }
            finally
            {
                _refreshOrderUid = Guid.Empty;
            }
        }
    }
}