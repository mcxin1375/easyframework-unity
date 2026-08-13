/*----------------------------------------------------------------
// author:Cookie mcx
// date:2023/4/14
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace EasyFramework
{
    public abstract class UIToolkitListViewEx<T>
    {
        public int SelectIndex { get; private set; }
        public T[] Datas { get; private set; }
        public ListView ListView { get; private set; }

        private readonly Action<int, T> _itemAction = null;
        
        protected UIToolkitListViewEx(ListView listView, Action<int, T> itemAction = null)
        {
            ListView = listView;
            _itemAction = itemAction;
            
            ListView.makeItem += OnMakeItem;
            ListView.bindItem += BindItem;
            ListView.onSelectionChange += OnSelectionChange;
        }
        
        private void OnSelectionChange(IEnumerable<object> obj)
        {
            // Log.Info("OnSelectionChange", SelectIndex);
            // if (_selectIndex != SelectIndex)
            // {
            //     OnSelectItem(_selectIndex, false);
            // }
            // OnSelectItem(SelectIndex, true);
            // _selectIndex = SelectIndex;
        }

        public void RefreshData(T[] datas)
        {
            Datas = datas;
            ListView.itemsSource = datas;
            ListView.RefreshItems();
        }

        public void SelectAt(int index)
        {
            if (index != SelectIndex)
            {
                OnSelectItem(SelectIndex, false);
            }
            SelectIndex = index;
            OnSelectItem(SelectIndex, true);
        }

        public T GetData(int index)
        {
            if (Datas?.Length > index && index >= 0)
            {
                return Datas[index];
            }

            return default(T);
        }

        private void BindItem(VisualElement ve, int index)
        {
            ve.name = index.ToString();
            
            OnBindItem(ve, index);
        }

        protected void BindItemClick(VisualElement itemRootVisualElement)
        {
            if (itemRootVisualElement != null && int.TryParse(itemRootVisualElement.name, out int index))
            {
                SelectAt(index);
                OnItemAction(index);
            }
        }

        protected virtual VisualElement OnMakeItem()
        {
            Button button = new Button()
            {
                style = { unityTextAlign = TextAnchor.MiddleCenter } 
            };
            button.clicked += () =>
            {
                BindItemClick(button);
            };
            return button;
        }

        protected virtual void OnBindItem(VisualElement ve, int index)
        {
            Button btn = ve as Button;
            if (btn != null)
            {
                btn.text = index.ToString();
            }
        }

        protected virtual void OnSelectItem(int index, bool isSelect)
        {
            VisualElement ve = ListView.Q(index.ToString());
            // Log.Info(index, isSelect, ve);
            if (ve != null)
            {
                ve.style.backgroundColor = isSelect ? Color.gray : new Color(88 / 255f, 88 / 255f, 88 / 255f, 1);
            }
        }

        protected virtual void OnItemAction(int index)
        {
            T data = GetData(index);
            _itemAction?.Invoke(index, data);
        }

    }
}