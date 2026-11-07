/*----------------------------------------------------------------
// author:Cookie mcx
// date:2023/4/14
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine.UIElements;


namespace EasyFramework
{
    public interface IUIToolkitTabItem<T>
    {
        int TabIndex { get; }
        void BindObject(VisualElement visualElement, Action<IUIToolkitTabItem<T>> callback = null);
        void SetActive(bool value);
        void SetSelect(bool value);
        void RefreshData(T t, int index);
    }

    public abstract class UIToolkitTabItem<T> : IUIToolkitTabItem<T>
    {
        public VisualElement VisualElement { get; private set; }
        public T Data { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsSelect { get; private set; }
        public int TabIndex { get; private set; }

        public void BindObject(VisualElement visualElement, Action<IUIToolkitTabItem<T>> callback = null)
        {
            VisualElement = visualElement;
            if (callback != null)
            {
                VisualElement.RegisterCallback<ClickEvent>((e) => { callback?.Invoke(this); });
                // if (btn != null)
                // {
                //     btn.clickable.clicked += () => { callback?.Invoke(this); };
                // }
            }
        }

        public void RefreshData(T t, int index)
        {
            Data = t;
            TabIndex = index;
            OnRefresh();
        }

        public void SetActive(bool value)
        {
            IsActive = value;
            VisualElement.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            OnSetActive(value);
        }

        public void SetSelect(bool value)
        {
            IsSelect = value;
            OnSetSelect(value);
        }

        protected virtual void OnSetActive(bool value)
        {
        }

        protected virtual void OnSetSelect(bool value)
        {
        }

        protected abstract void OnRefresh();

    }
}