/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public struct AudioObject
    {
        private readonly Guid _uid;
        private readonly AudioItemBehaviour _audioBehaviour;
        
        internal AudioObject(Guid uid, AudioItemBehaviour behaviour)
        {
            _uid = uid;
            _audioBehaviour = behaviour;
        }
        public static AudioObject Empty => new AudioObject(Guid.Empty, null);

        public void Stop()
        {
            if (_audioBehaviour == null || _uid != _audioBehaviour.Uid) return;
            _audioBehaviour.Stop();
        }

        public bool IsPlaying()
        {
            if (_audioBehaviour == null || _uid != _audioBehaviour.Uid) return false;
            return _audioBehaviour.IsPlaying;
        }
    }
}