/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;

namespace EasyFramework
{
    public class UIBaseBehaviour : MonoBehaviour
    {
        [System.NonSerialized] private RectTransform _rectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        // 只在编辑器下生效一次，运行时不生效，单纯辅助减少手动绑定
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            OnValidateEditorEx();
        }

        protected virtual void OnValidateEditorEx()
        {
            try
            {
                UnityComponentHelper.AutoSetComponents(this, gameObject);
            }
            catch (Exception e)
            {
            }
        }
        
    }
}