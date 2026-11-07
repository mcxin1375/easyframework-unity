using System.Collections.Generic;

namespace EasyFramework
{
    public class ControllerLoading : IObjectTask
    {
        public bool IsCompleted => !IsLoading;
        public bool IsLoading => GetIsLoading();
        public float Progress => GetProgress();
        public int Weight { get; private set; } = 100;

        private readonly HashSet<IControllerLoading> _handlers = new();

        public void Clear()
        {
            _handlers.Clear();
        }

        public ETask StartLoadingAsync()
        {
            foreach (var handler in _handlers) handler.OnStartLoading();
            
            return this.WaitTaskCompleted();
        }

        public void Add(IControllerLoading handler)
        {
            if (handler == null) return;
            _handlers.Add(handler);
            Weight = 0;
            foreach (var loadingHandler in _handlers) Weight += loadingHandler.Weight;
        }

        private bool GetIsLoading()
        {
            foreach (var handler in _handlers)
                if (handler.IsLoading) return true;
            return false;
        }

        private float GetProgress()
        {
            float progress = 0;
            foreach (var handler in _handlers)
            {
                // Debug.Log($"{handler.GetType().Name} - {handler.IsLoading}, {handler.Progress}");
                var p = handler.IsLoading ? handler.Progress : 1;
                var weightRate = handler.Weight / (float)Weight;
                progress += p * weightRate;
            }

            return progress;
        }
    }
}
