/*----------------------------------------------------------------
// author:Cookie mcx
// date:2023/4/14
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using UnityEngine.UIElements;


namespace EasyFramework
{
    public class UIToolkitTabEx<T, K> where T : class, IUIToolkitTabItem<K>, new()
    {

        public int SelectIndex { get; private set; }
        public K[] Datas { get; private set; }

        public VisualElement VisualElement { get; private set; }


        private readonly List<IUIToolkitTabItem<K>> _items = new List<IUIToolkitTabItem<K>>();
        private readonly Action<T> _itemAction = null;

        public UIToolkitTabEx(VisualElement visualElement, Action<T> itemAction = null)
        {
            VisualElement = visualElement;
            _itemAction = itemAction;

            for (int i = 0; i < VisualElement.childCount; i++)
            {
                T t = new T();
                t.BindObject(VisualElement[i], OnItemAction);
                t.SetActive(false);
                _items.Add(t);
            }
        }

        public void RefreshData(K[] datas)
        {
            Datas = datas;

            foreach (IUIToolkitTabItem<K> item in _items)
            {
                item.SetActive(false);
            }

            if (Datas == null) return;

            for (int i = 0; i < Datas.Length; i++)
            {
                // 动态创建，暂时注释
                // if (i >= _items.Count)
                // {
                //     T t = new T();
                //
                //     VisualTreeAsset asset =
                //         AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/EditorWindow/MapEditorWindow/UIDocument/MapEditorTabItem.uxml");
                //     TemplateContainer tmp = asset.Instantiate();
                //     VisualElement.Add(tmp);
                //     t.BindObject(tmp, OnItemAction);
                //     _items.Add(t);
                // }

                IUIToolkitTabItem<K> item = _items[i];
                item.SetActive(true);
                item.RefreshData(Datas[i], i);
            }

            SelectAt(SelectIndex);
        }

        public void SelectAt(int index)
        {
            GetItem(SelectIndex)?.SetSelect(false);
            SelectIndex = index;

            IUIToolkitTabItem<K> item = GetItem(SelectIndex);
            item?.SetSelect(true);
            _itemAction?.Invoke(item as T);
        }

        public IUIToolkitTabItem<K> GetItem(int index)
        {
            if (_items.Count > index && index >= 0)
            {
                return _items[index];
            }

            return null;
        }

        private void OnItemAction(IUIToolkitTabItem<K> item)
        {
            SelectAt(item?.TabIndex ?? SelectIndex);
        }


    }
}