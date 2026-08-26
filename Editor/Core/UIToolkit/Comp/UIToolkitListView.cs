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
    public class UIToolkitListView<T>
    {
        public T[] TArray { get; private set; }
        public int SelectedIndex => ListView?.selectedIndex ?? -1;
        
        protected ListView ListView { get; }

        private readonly Dictionary<VisualElement, int> _itemIndexDict = new Dictionary<VisualElement, int>();

        private readonly System.Action<int> _onSelectedIndexChanged;
        
        public UIToolkitListView(ListView listView, System.Action<int> onSelectedIndexChanged)
        {
            ListView = listView;
            _onSelectedIndexChanged = onSelectedIndexChanged;
            
            ListView.makeItem += CreateItem;
            ListView.bindItem += RefreshItem;
            ListView.itemIndexChanged += OnItemIndexChanged;
            ListView.selectionChanged += OnSelectionChanged;
        }
        
        public void RefreshData(T[] tArray)
        {
            _itemIndexDict.Clear();
            
            TArray = tArray;
            ListView.itemsSource = tArray;
            ListView.RefreshItems();
        }
        // public void RefreshData(List<T> tList)
        // {
        //     _itemIndexDict.Clear();
        //     
        //     TList = tList;
        //     ListView.itemsSource = tList;
        //     ListView.RefreshItems();
        // }

        public void SelectAt(int index)
        {
            ListView.selectedIndex = index;
            _onSelectedIndexChanged?.Invoke(index);
        }

        public int GetItemIndex(VisualElement item) => _itemIndexDict.ContainsKey(item) ? _itemIndexDict[item] : -1;

        public T GetData(int index)
        {
            if (TArray?.Length > index && index >= 0) return TArray[index];
            return default(T);
        }
        
        protected virtual VisualElement CreateItem()
        {
            Button item = new Button()
            {
                style = { unityTextAlign = TextAnchor.MiddleLeft } 
            };
            item.clicked += () =>
            {
                SelectAt(GetItemIndex(item));
            };

            return item;
        }

        private void RefreshItem(VisualElement ve, int index)
        {
            if (_itemIndexDict.ContainsKey(ve)) _itemIndexDict[ve] = index;
            else _itemIndexDict.Add(ve, index);
            
            OnRefreshItem(ve, index, GetData(index));
        }

        protected virtual void OnRefreshItem(VisualElement ve, int index, T t)
        {
            if (ve is Button btn)
            {
                btn.text = t.ToString();
            }
        }

        protected virtual void OnItemIndexChanged(int arg1, int arg2)
        {
            // Log.Info("OnItemIndexChanged", arg1, arg2);
        }

        protected virtual void OnSelectionChanged(IEnumerable<object> obj)
        {
            // Log.Info("OnSelectionChanged");
        }
    }
}