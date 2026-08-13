/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/8/15
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace EasyFramework
{
    public class UIScaleBehaviour : MonoBehaviour
    {
        public enum UIScaleType
        {
            FitInside,
            FitOutside,
            Stretch,
        }

        public UIScaleType scaleType = UIScaleType.FitOutside;
        public Vector2 textureSize;
        public float scaleFactor = 1.0f;
        public bool forceUpdateCanvases;
        public bool editorRefresh = true;
        
        private RectTransform _rectTransform;
        private Canvas _canvas;

        private void Start()
        {
            if (forceUpdateCanvases) Canvas.ForceUpdateCanvases();
            Refresh();
        }

        public void Refresh()
        {
            if (_rectTransform == null) _rectTransform = transform as RectTransform;
            if (_rectTransform == null) return;
            
            if (scaleType == UIScaleType.Stretch)
            {
                _rectTransform.anchorMin = Vector2.zero;
                _rectTransform.anchorMax = Vector2.one;
                _rectTransform.pivot = new Vector2(0.5f, 0.5f);
                _rectTransform.anchoredPosition3D = Vector3.zero;
                _rectTransform.sizeDelta = Vector2.zero;
                _rectTransform.localScale = Vector3.one;
                return;
            }
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null) return;
            
            if (!TryGetSpriteSize(out textureSize)) return;

            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition3D = Vector3.zero;
            _rectTransform.localScale = Vector3.one;
            
            RectTransform parent = _canvas.transform as RectTransform;
            if (parent == null) return;
            Vector2 parentSize = parent.rect.size;

            float scaleX = parentSize.x / textureSize.x;
            float scaleY = parentSize.y / textureSize.y;

            switch (scaleType)
            {
                case UIScaleType.FitInside:
                    scaleFactor = Mathf.Min(scaleX, scaleY);
                    break;
                case UIScaleType.FitOutside:
                    scaleFactor = Mathf.Max(scaleX, scaleY);
                    break;
                default: scaleFactor = 1; break;
            }

            _rectTransform.sizeDelta = textureSize * scaleFactor;
            
            // Debug.Log($"{textureSize}, {parentSize}, {scaleFactor}");
        }

        private bool TryGetSpriteSize(out Vector2 size)
        {
            var image = GetComponent<Image>();
            if (image != null && image.sprite != null)
            {
                size = image.sprite.rect.size;
                return true;
            }
            var rawImage = GetComponent<RawImage>();
            if (rawImage != null && rawImage.texture != null)
            {
                size = new Vector2(rawImage.texture.width, rawImage.texture.height);
                return true;
            }
            size = Vector2.zero;
            return false;
        }
    }
}