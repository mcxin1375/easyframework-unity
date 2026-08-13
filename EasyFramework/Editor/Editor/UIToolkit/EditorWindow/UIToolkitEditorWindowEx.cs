/*----------------------------------------------------------------
// author: Cookie(mcx)
// date: 2023/12/28
// describe: 
//----------------------------------------------------------------*/

using UnityEngine.UIElements;

namespace EasyFramework.Editor
{
    public abstract class UIToolkitEditorWindowEx<T> : UIToolkitEditorWindowEx where T : UIToolkitEditorWindow
    {
        protected new T Window { get; private set; }
        
        internal override void Create(UIToolkitEditorWindow window)
        {
            Window = window as T;
            base.Create(window);
        }
    }

    public abstract class UIToolkitEditorWindowEx
    {
        protected UIToolkitEditorWindow Window { get; private set; }

        internal virtual void Create(UIToolkitEditorWindow window)
        {
            Window = window;
            OnCreate();
        }
        internal void Dispose()
        {
            OnDispose();
        }
        internal void Open() => OnOpen();
        internal void Close() => OnClose();
        internal void AddListeners() => OnAddListeners();
        internal void RemoveListeners() => OnRemoveListeners();
        internal void ButtonClick(Button btn) => OnButtonClick(btn);
        
        protected virtual void OnCreate() { }
        protected virtual void OnDispose() { }
        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnAddListeners() { }
        protected virtual void OnRemoveListeners() { }
        protected virtual void OnButtonClick(Button btn) { }
    }
}