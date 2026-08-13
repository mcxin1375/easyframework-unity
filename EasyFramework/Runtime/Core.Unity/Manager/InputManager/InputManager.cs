/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;

namespace EasyFramework
{
    public enum EInputType
    {
        Down,
        Hover,
        Up,
    }

    public class InputManager : Singleton<InputManager>, IInputManager, FBehaviour.IEvent
    {
        public event Action<EInputType, int, Vector2> OnInputEvent;

        public InputManager()
        {
            Input.multiTouchEnabled = true;
            FBehaviour.Instance.Register(this);
        }
        void FBehaviour.IEvent.OnUpdate()
        {
            OnTick();
        }
        void FBehaviour.IEvent.OnLateUpdate()
        {
            
        }
        void FBehaviour.IEvent.OnDestroy()
        {
        }

        void OnTick()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    UpdateTouch();
                    if (Input.touchCount == 0)
                        UpdateMouse();
                    break;
                default:
                    UpdateTouch();
                    break;
            }
        }

        private void UpdateMouse()
        {
            for (int i = 0; i < 3; i++)
            {
                int fingerId = i + 1000;
                if (Input.GetMouseButtonDown(i))
                {
                    Dispatch(EInputType.Down, fingerId, Input.mousePosition);
                }
                else if (Input.GetMouseButton(i))
                {
                    Dispatch(EInputType.Hover, fingerId, Input.mousePosition);
                }
                else if (Input.GetMouseButtonUp(i))
                {
                    Dispatch(EInputType.Up, fingerId, Input.mousePosition);
                }
            }
        }

        private void UpdateTouch()
        {
            if (Input.touchCount > 0)
            {
                foreach (Touch touch in Input.touches)
                {
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            Dispatch(EInputType.Down, touch.fingerId, touch.position);
                            break;
                        case TouchPhase.Stationary:
                        case TouchPhase.Moved:
                            Dispatch(EInputType.Hover, touch.fingerId, touch.position);
                            break;
                        case TouchPhase.Ended:
                        case TouchPhase.Canceled:
                            Dispatch(EInputType.Up, touch.fingerId, touch.position);
                            break; ;
                    }
                }
            }
        }

        private void Dispatch(EInputType type, int fingerId, Vector2 position)
        {
            OnInputEvent?.Invoke(type, fingerId + 1, position);
        }
    }
}