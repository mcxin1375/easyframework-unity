/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyFramework
{
    public enum EAudioType
    {
        Audio = 0,
        Music = 1,
    }
    [Serializable]
    public class AudioChannelSettings
    {
        public bool mute;
        public float volume = 1;
    }
    internal class AudioPlayer : SingletonBehaviour<AudioPlayer>, IAudioPlayer
    {
        public AudioListener AudioListener { get; private set; }

        private readonly Dictionary<EAudioType, AudioChannelSettings> _channelDict = new();
        private readonly List<AudioItemBehaviour> _playingList = new();

        private static AudioItemBehaviour CreateFunc()
        {
            return new GameObject(nameof(AudioItemBehaviour)).AddComponent<AudioItemBehaviour>();
        }

        void Awake()
        {
            ObjectPoolItem<AudioItemBehaviour>.Shared.CreateFunc = CreateFunc;
            
            transform.SetParent(FBehaviour.Instance.transform);
            
            var listenerArr = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            foreach (var value in listenerArr) value.enabled = false;
            
            AudioListener = gameObject.AddComponent<AudioListener>();

            foreach (EAudioType value in Enum.GetValues(typeof(EAudioType)))
            {
                var settings = new AudioChannelSettings();
                _channelDict.Add(value, settings);
            }
        }

        void Update()
        {
            for (int i = _playingList.Count - 1; i >= 0; i--)
            {
                var behaviour = _playingList[i];
                if (behaviour.IsPlaying) continue;
                _playingList.RemoveAt(i);
                ObjectPoolItem<AudioItemBehaviour>.Shared.Return(behaviour);
            }
        }

        public AudioObject PlayAudio(string clipName, bool isLoop = false) => Play(clipName, isLoop, EAudioType.Audio);
        public AudioObject PlayMusic(string clipName, bool isLoop = false) => Play(clipName, isLoop, EAudioType.Music);
        public AudioObject Play(string clipName, bool loop, EAudioType audioType)
        {
            var sourceBehaviour = ObjectPoolItem<AudioItemBehaviour>.Shared.Rent();
            sourceBehaviour.audioSource.loop = loop;
            sourceBehaviour.audioSource.volume = GetVolume(audioType);
            sourceBehaviour.audioSource.mute = GetMute(audioType);
            sourceBehaviour.Play(clipName, audioType);
            _playingList.Add(sourceBehaviour);
            return sourceBehaviour.ToAudioObject();
        }

        public int GetClipCount(string clipName)
        {
            int clipCount = 0;
            for (int i = _playingList.Count - 1; i >= 0; i--)
            {
                var behaviour = _playingList[i];
                if (!behaviour.IsPlaying) continue;
                if (behaviour.audioSource.clip.name == clipName) clipCount++;
            }
            return clipCount;
        }

        public bool IsPlaying(string clipName)
        {
            foreach (var behaviour in _playingList)
            {
                if (!behaviour.IsPlaying) continue;
                if (behaviour.audioSource.clip.name == clipName) return true;
            }
            return false;
        }

        public void StopAll()
        {
            foreach (var behaviour in _playingList) ObjectPoolItem<AudioItemBehaviour>.Shared.Return(behaviour);
            _playingList.Clear();
        }
        public void StopAll(EAudioType audioType)
        {
            for (int i = _playingList.Count - 1; i >= 0; i--)
            {
                var behaviour = _playingList[i];
                if (behaviour.AudioType != audioType) continue;
                _playingList.RemoveAt(i);
                ObjectPoolItem<AudioItemBehaviour>.Shared.Return(behaviour);
            }
        }

        public bool GetMute(EAudioType audioType) => _channelDict[audioType].mute;
        public float GetVolume(EAudioType audioType) => _channelDict[audioType].volume;
        public void SetMute(EAudioType audioType, bool value)
        {
            _channelDict[audioType].mute = value;
            foreach (var behaviour in _playingList)
            {
                if (behaviour.AudioType != audioType) continue;
                behaviour.audioSource.mute = value;
            }
        }
        public void SetVolume(EAudioType audioType, float value)
        {
            _channelDict[audioType].volume = value;
            foreach (var behaviour in _playingList)
            {
                if (behaviour.AudioType != audioType) continue;
                behaviour.audioSource.volume = value;
            }
        }
    }
}