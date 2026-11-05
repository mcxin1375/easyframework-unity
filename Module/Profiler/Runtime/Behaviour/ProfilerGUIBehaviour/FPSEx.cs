using UnityEngine;

namespace EasyFramework.Profiler
{
    public class FPSEx
    {
        public float FpsInterval { get; set; } = 0.5f;
        public int FPS { get; private set; }

        private int _frameCount;
        private float _timePassed;

        public void OnUpdate()
        {
            _frameCount ++;
            _timePassed += Time.deltaTime;
            if (_timePassed > FpsInterval)
            {
                FPS = (int)(_frameCount / _timePassed);
                _timePassed = 0.0f;
                _frameCount = 0;
            }
        }
    }
}