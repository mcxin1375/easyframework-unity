/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework
{
    public interface IAudioPlayer
    {
        AudioListener AudioListener { get; }

        AudioObject PlayAudio(string clipName, bool isLoop = false);
        AudioObject PlayMusic(string clipName, bool isLoop = false);
        AudioObject Play(string clipName, bool loop, EAudioType audioType);

        int GetClipCount(string clipName);
        bool IsPlaying(string clipName);
        
        void StopAll();
        void StopAll(EAudioType audioType);
        bool GetMute(EAudioType audioType);
        float GetVolume(EAudioType audioType);
        void SetMute(EAudioType audioType, bool value);
        void SetVolume(EAudioType audioType, float value);
    }
}