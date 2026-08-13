/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;
using UnityEngine.UI;

namespace EasyFramework
{
    public class UIGridItemBehaviour<T> : UIBaseBehaviour, ITimerObject
    {
        public ref readonly T TValue => ref _value;
        private T _value;

        [SerializeField] private int index = -1;
        public int Index => index;
        
        [SerializeField] private bool isSelected;
        public bool IsSelected => isSelected;
        
        public bool IsTimerAlive => gameObject != null && gameObject.activeSelf;
        
        protected IUIScrollRectList UIScrollRectList { get; private set; }

        internal void Create(IUIScrollRectList behaviour)
        {
            UIScrollRectList = behaviour;
            UnityComponentHelper.AutoSetComponents(this, gameObject);
            
            var uiButtonBinding = gameObject.AddComponent<UIButtonBinding>();
            uiButtonBinding.BindAction(OnButtonClick);
            
            OnCreate();
        }
        
        internal void Refresh(T value, int i, bool selected)
        {
            _value = value;
            index = i;
            
            SetSelect(selected);
            OnRefresh();
        }
        
        internal void SetActive(bool active)
        {
            gameObject.SetActive(active);
            OnSetActive(active);
        }
        
        internal void Destroy()
        {
            OnDestroyEx();
            GameObject.Destroy(gameObject);
        }

        internal void SetSelect(bool selected)
        {
            isSelected = selected;
            OnSelectState(selected);
        }
        
        protected virtual void OnCreate() { }
        protected virtual void OnDestroyEx() { }
        protected virtual void OnRefresh() { }
        protected virtual void OnSetActive(bool active) { }
        protected virtual void OnSelectState(bool selected) { }
        protected virtual void OnButtonClick(Button btn)
        {
            UIScrollRectList.OnItemButtonClick(btn, index);
        }
    }
}