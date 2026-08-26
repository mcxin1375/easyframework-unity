/*----------------------------------------------------------------
// author:Cookie mcx
// date:2023/4/14
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyFramework.Editor
{
    public abstract class UIToolkitListViewEx<T, K> : UIToolkitListViewEx<T> where T : UIToolkitEditorWindow
    {
        public K[] Items { get; private set; }

        public void Refresh(K[] items)
        {
            Items = items;
            
            OnRefresh();
        }
        
        protected override void OnRefresh()
        {
            ListView.itemsSource = Items;
            ListView.RefreshItems();
        }
        
        protected override void BindItem(VisualElement ve, int index)
        {
            (ve as Label).text = Items[index].ToString();
        }

        protected override VisualElement MakeItem()
        {
            return new Label()
            {
                style = { unityTextAlign = TextAnchor.MiddleLeft } 
            };
        }
    }

    public abstract class UIToolkitListViewEx<T> : UIToolkitEditorWindowEx<T> where T : UIToolkitEditorWindow
    {
        protected abstract string ViewName { get; }
        protected ListView ListView { get; private set; }

        protected override void OnCreate()
        {
            base.OnCreate();
            
            ListView = Window.rootVisualElement.Q<ListView>(ViewName);
            if (ListView == null)
            {
                Debug.LogError($"{typeof(T)} find listView error");
                return;
            }
            
            ListView.makeItem += MakeItem;
            ListView.bindItem += BindItem;
            ListView.itemIndexChanged += OnItemIndexChanged;
            ListView.selectionChanged += OnSelectionChanged;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            
            if (ListView == null) return;
            
            ListView.makeItem -= MakeItem;
            ListView.bindItem -= BindItem;
            ListView.itemIndexChanged -= OnItemIndexChanged;
            ListView.selectionChanged -= OnSelectionChanged;
            ListView = null;
        }

        public void Refresh() => OnRefresh();
        
        protected virtual void OnItemIndexChanged(int arg1, int arg2)
        {
            // Log.Info("OnItemIndexChanged", arg1, arg2);
        }
        protected virtual void OnSelectionChanged(IEnumerable<object> obj)
        {
            // Log.Info("OnSelectionChanged");
        }

        protected abstract void OnRefresh();
        protected abstract void BindItem(VisualElement ve, int index);
        protected abstract VisualElement MakeItem();
    }
}