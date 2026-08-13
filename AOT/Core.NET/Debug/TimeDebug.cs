using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EasyFramework
{
    public readonly struct TimeDebug
    {
        private readonly string _timeTag;
        private readonly Guid _token;
        
        private TimeDebug(string tag, Guid token)
        {
            _timeTag = tag;
            _token = token;
        }

        public void StopAndPrint(float minValue = 0.01f)
        {
            if (Dict.Remove(_token, out var sw))
            {
                sw.Stop();
                var value = sw.ElapsedMilliseconds / 1000f;
                if (value > minValue)
                {
                    FDebug.Log($"{_timeTag} - {sw.ElapsedMilliseconds / 1000f}s");
                }
                ObjectPool<Stopwatch>.Shared.Return(sw);
            }
        }

        private static readonly Dictionary<Guid, Stopwatch> Dict = new();
        public static TimeDebug Start(string tag)
        {
            var token = Guid.NewGuid();
            var sw = ObjectPool<Stopwatch>.Shared.Rent();
            Dict.Add(token, sw);
            sw.Restart();
            return new TimeDebug(tag, token);
        }
    }
}