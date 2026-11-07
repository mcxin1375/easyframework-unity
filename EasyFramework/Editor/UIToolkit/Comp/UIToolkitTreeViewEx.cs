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
    public abstract class UIToolkitTreeViewEx<T, K> : UIToolkitTreeViewEx<T> where T : UIToolkitEditorWindow
    {
        public K[] Items { get; private set; }
        public void RefreshList(K[] items)
        {
            Items = items;
            Refresh();
        }
        protected abstract void Refresh();
    }

    public abstract class UIToolkitTreeViewEx<T> : UIToolkitEditorWindowEx<T> where T : UIToolkitEditorWindow
    {
        protected abstract string ViewName { get; }
        protected TreeView TreeView { get; private set; }

        protected override void OnCreate()
        {
            base.OnCreate();

            TreeView = Window.rootVisualElement.Q<TreeView>(ViewName);
            if (TreeView == null)
            {
                Debug.LogError($"{typeof(T)} find treeView error");
                return;
            }

            TreeView.makeItem += MakeItem;
            TreeView.bindItem += BindItem;
            TreeView.itemIndexChanged += OnItemIndexChanged;
            TreeView.selectionChanged += OnSelectionChanged;
            TreeView.itemsChosen += OnItemsChosen;
            TreeView.selectedIndicesChanged += OnSelectedIndicesChanged;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            
            if (TreeView == null) return;
            
            TreeView.makeItem -= MakeItem;
            TreeView.bindItem -= BindItem;
            TreeView.itemIndexChanged -= OnItemIndexChanged;
            TreeView.selectionChanged -= OnSelectionChanged;
            TreeView.itemsChosen -= OnItemsChosen;
            TreeView.selectedIndicesChanged -= OnSelectedIndicesChanged;
        }

        protected virtual void OnItemsChosen(IEnumerable<object> obj)
        {
            // Log.Info("OnItemsChosen");
        }
        protected virtual  void OnSelectedIndicesChanged(IEnumerable<int> obj)
        {
            // Log.Info("OnSelectedIndicesChanged");
        }
        protected virtual void OnItemIndexChanged(int arg1, int arg2)
        {
            // Log.Info("OnItemIndexChanged", arg1, arg2);
        }
        protected virtual void OnSelectionChanged(IEnumerable<object> obj)
        {
            // Log.Info("OnSelectionChanged");
        }
        
        protected abstract void BindItem(VisualElement ve, int index);
        protected abstract VisualElement MakeItem();
        
    }

}