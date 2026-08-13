/*----------------------------------------------------------------
// author:meng cheng xin
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EasyFramework
{
    public class UIEventBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IEndDragHandler, IDragHandler, IBeginDragHandler
    {

        public Action<PointerEventData> OnDragAction;
        public Action<PointerEventData> OnEndDragAction;
        public Action<PointerEventData> OnBeginAction;
        public Action<PointerEventData> OnPointerDownAction;
        public Action<PointerEventData> OnPointerUpAction;

        public virtual void OnDrag(PointerEventData eventData)
        {
            OnDragAction?.Invoke(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragAction?.Invoke(eventData);
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginAction?.Invoke(eventData);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDownAction?.Invoke(eventData);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpAction?.Invoke(eventData);
        }

    }
}