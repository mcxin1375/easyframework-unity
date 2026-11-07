using UnityEngine.UIElements;

namespace EasyFramework
{
    public abstract class UIToolkitBaseWindow
    {

        public bool IsOpen { get; private set; }

        protected VisualElement RootVisualElement { get; private set; }

        internal void Create()
        {
            OnCreate();
        }
        internal void Dispose()
        {
            OnDispose();
        }
        internal void Open()
        {
            if (!IsOpen)
            {
                IsOpen = true;
                OnAddListeners();
            }
            OnOpen();
        }
        internal void Close()
        {
            if (IsOpen)
            {
                IsOpen = false;
                OnRemoveListeners();
            }
            OnClose();
        }


        protected virtual void OnCreate() { }
        protected virtual void OnDispose() { }
        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnAddListeners() { }
        protected virtual void OnRemoveListeners() { }


    }
}