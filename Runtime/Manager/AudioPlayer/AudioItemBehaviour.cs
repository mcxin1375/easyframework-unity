/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;

namespace EasyFramework
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioItemBehaviour : MonoBehaviour, IObjectPoolEvent
    {
        public bool IsPlaying => audioSource.clip != null && audioSource.isPlaying;
        public EAudioType AudioType => audioType;
        public string AudioName => audioName;
        public Guid Uid => _uid;
        
        [SerializeField] internal AudioSource audioSource;
        [SerializeField] private string audioName;
        [SerializeField] private EAudioType audioType;
        private Guid _uid;

        private void Awake()
        {
            transform.SetParent(AudioPlayer.Instance.transform);
            audioSource = gameObject.AddComponentEx<AudioSource>();
        }

        void IObjectPoolEvent.OnReturn()
        {
            Stop();
            transform.SetParent(AudioPlayer.Instance.transform);
        }

        void IObjectPoolEvent.OnDispose()
        {
            Stop();
            Destroy(gameObject);
        }

        public void Play(string resName, EAudioType channel)
        {
            _uid = Guid.NewGuid();
            
            transform.name = resName;
            audioName = resName;
            audioType = channel;
            
            audioSource.clip = LoadAudioClip(resName);
            if (audioSource.clip == null)
            {
                FDebug.LogError($"player audio error: {resName}");
                return;
            }
            audioSource.Play();
        }
        
        public void Stop()
        {
            transform.name = nameof(AudioItemBehaviour);
            _uid = Guid.Empty;
            audioSource.Stop();
            audioSource.clip = null;
        }

        public AudioObject ToAudioObject() => new AudioObject(_uid, this);

        private AudioClip LoadAudioClip(string resName)
        {
            if (string.IsNullOrEmpty(resName)) return null;
            return F.ResLoader.LoadAsset<AudioClip>(resName);
        }
    }
}