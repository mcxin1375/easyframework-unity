/*----------------------------------------------------------------
// author:meng cheng xin
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace EasyFramework
{
    public class UIButtonBinding : MonoBehaviour
    {
        private Button[] _btnArr;
        private Action<Button> _action;

        private void Awake()
        {
            AddButtonBinding();
        }
        private void OnDestroy()
        {
            RemoveButtonBinding();
        }
        public void BindAction(Action<Button> callback)
        {
            _action = callback;
        }
        private void AddButtonBinding()
        {
            _btnArr = transform.GetComponentsInChildren<Button>(true);
            if (_btnArr != null && _btnArr.Length > 0)
            {
                foreach (Button btn in _btnArr)
                {
                    btn.onClick.AddListener(() =>
                    {
                        OnButtonClick(btn);
                    });
                }
            }
        }
        private void RemoveButtonBinding()
        {
            if (_btnArr != null && _btnArr.Length > 0)
            {
                foreach (Button btn in _btnArr)
                {
                    btn.onClick.RemoveAllListeners();
                }
            }
        }
        protected virtual void OnButtonClick(Button btn)
        {
            // Log.Info(btn.name);
            
            _action?.Invoke(btn);
        }
    }
}