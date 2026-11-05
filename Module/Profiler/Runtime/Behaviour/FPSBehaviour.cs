using UnityEngine;

namespace EasyFramework.Profiler
{
    public class FPSBehaviour : MonoBehaviour
    {
        public int FPS => fps;
        
        [SerializeField] private int fps;
        [SerializeField] private float fpsInterval = 0.5f;
        public bool guiEnabled = true;

        private int _frameCount;
        private float _timePassed;

        void Update()
        {
            _frameCount ++;
            _timePassed += Time.deltaTime;
            if (_timePassed > fpsInterval)
            {
                fps = (int)(_frameCount / _timePassed);
                _timePassed = 0.0f;
                _frameCount = 0;
            }
        }

        private void OnGUI()
        {
            if (!guiEnabled) return;
            GUI.TextField(new Rect(0, 0, 100, 50), $"FPS: {FPS}");
        }
    }
}