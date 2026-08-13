/*----------------------------------------------------------------
// author:meng cheng xin
// date:2018/3/1
// describe:UI管理类
//----------------------------------------------------------------*/

using UnityEngine;
using UnityEngine.UI;

namespace EasyFramework
{
    public class UILayerBehaviour : UIBaseBehaviour
    {
        public UILayer Layer => layer;
        public Canvas Canvas => canvas;
        public CanvasScaler CanvasScaler => canvasScaler;
        public Camera WorldCamera => Canvas.worldCamera;
        public string SortingLayerName => Canvas.sortingLayerName;
        public int OrderInterval => orderInterval;
        
        [SerializeField] private UILayer layer;
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private int orderInterval = 10;
        
        private void Awake()
        {
            canvas ??= gameObject.GetComponent<Canvas>();
            canvasScaler ??= gameObject.GetComponent<CanvasScaler>();
        }

        protected override void OnValidateEditorEx()
        {
            base.OnValidateEditorEx();
            
            Awake();
            
            canvas = gameObject.GetComponent<Canvas>();
            canvasScaler = gameObject.GetComponent<CanvasScaler>();
            switch (gameObject.name)
            {
                case "HUDLayer": layer = UILayer.HUD; break;
                case "PopupLayer": layer = UILayer.Popup; break;
                case "NoticeLayer": layer = UILayer.Notice; break;
                case "HigherLayer": layer = UILayer.Higher; break;
                case "LoadingLayer": layer = UILayer.Loading; break;
            }
        }

        public void SetResolution(Vector2 resolution)
        {
            CanvasScaler.referenceResolution = resolution;
        }
    }
}