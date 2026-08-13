/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace EasyFramework
{
    public enum EUIRenderMode
    {
        UICamera,
        Overlay
    }
    internal class UIRootBehaviour : UIBaseBehaviour
    {
        private static UIRootBehaviour _instance;
        public static UIRootBehaviour Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = EasyFrameworkAOTSettings.Instance.uiRoot ?? Resources.Load<GameObject>("UIRoot");
                    var uiRoot = Object.Instantiate(obj);
                    _instance = uiRoot.GetComponent<UIRootBehaviour>();
                }
                return _instance;
            }
        }

        public Vector2 Resolution
        {
            get => resolution;
            set => SetResolution(value);
        }
        public EUIRenderMode UIRenderMode
        {
            get => uiRenderMode;
            set => SetRenderMode(value);
        }
        public Camera UICamera => uiCamera;
        public EventSystem EventSystem => eventSystem;
        public UILayerBehaviour[] UILayers => uiLayers;
        
        [SerializeField] private EUIRenderMode uiRenderMode = EUIRenderMode.UICamera;
        [SerializeField] private Vector2 resolution = new Vector2(1920, 1080);
        [SerializeField] private Camera uiCamera;
        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private UILayerBehaviour[] uiLayers;
        
        private readonly Dictionary<UILayer, UILayerBehaviour> _uiLayerDict = new();
        
        private void Awake()
        {
            Object.DontDestroyOnLoad(gameObject);

            // transform.position = Vector3.up * 2000;
            transform.name = "[UIRoot]";
            foreach (var uiLayerBehaviour in uiLayers)
            {
                uiLayerBehaviour.SetResolution(Resolution);
                _uiLayerDict.Add(uiLayerBehaviour.Layer, uiLayerBehaviour);
            }
            
            var eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            foreach (var es in eventSystems)
            {
                if (es != eventSystem) es.gameObject.SetActive(false);
            }
            eventSystem.gameObject.SetActive(true);

            Resolution = EasyFrameworkAOTSettings.Instance.resolution;
            UIRenderMode = EasyFrameworkAOTSettings.Instance.uiRenderMode;
        }

        protected override void OnValidateEditorEx()
        {
            base.OnValidateEditorEx();

            uiCamera = gameObject.GetComponentInChildren<Camera>(true);
            eventSystem = gameObject.GetComponentInChildren<EventSystem>(true);
            uiLayers = gameObject.GetComponentsInChildren<UILayerBehaviour>(true);
        }
        
        public Camera GetCamera(UILayer uiLayer) => uiCamera;
        public Canvas GetCanvas(UILayer uiLayer) => GetLayerData(uiLayer)?.Canvas;
        public UILayerBehaviour GetLayerData(UILayer uiLayer) => _uiLayerDict.GetValueOrDefault(uiLayer);
        public void SetResolution(Vector2 vector2)
        {
            resolution = vector2;
            foreach (var layer in uiLayers)
                layer.CanvasScaler.referenceResolution = vector2;
        }

        public void SetRenderMode(EUIRenderMode mode)
        {
            uiRenderMode = mode;
            
            switch (uiRenderMode)
            {
                case EUIRenderMode.Overlay:
                    uiCamera.gameObject.SetActive(false);
                    foreach (var uiLayer in uiLayers)
                    {
                        uiLayer.Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    }
                    
                    break;
                case EUIRenderMode.UICamera:
                    uiCamera.gameObject.SetActive(true);
                    foreach (var uiLayer in uiLayers)
                    {
                        uiLayer.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
                        uiLayer.Canvas.worldCamera = uiCamera;
                    }
                    break;
            }
        }

        internal void UpdateRootSettings()
        {
            if (uiCamera != null)
            {
                uiCamera.gameObject.SetActive(uiRenderMode == EUIRenderMode.UICamera);
            }
            if (uiLayers?.Length > 0)
            {
                foreach (var layer in uiLayers)
                {
                    layer.CanvasScaler.referenceResolution = resolution;
                    layer.Canvas.renderMode = uiRenderMode == EUIRenderMode.Overlay ? RenderMode.ScreenSpaceOverlay : RenderMode.ScreenSpaceCamera;
                }
            }
        }
    }
}