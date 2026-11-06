using UnityEngine;

namespace EasyFramework.Samples.Modules
{
    // 播放入口会等待初始化；Listener、查询、频道和停止方法须在初始化后调用。
    // 先准备资源构建目录中的 MyMusic / MyEffect；业务保存播放句柄并在结束时 Stop()。
    public static class AudioExample
    {
        public static AudioObject Empty => AudioObject.Empty;
        public static AudioListener GetListener() => F.AudioPlayer.AudioListener;

        public static async ETask<AudioObject> PlayAsync(string clipName, bool loop, EAudioType channel)
        {
            await F.InitializeAsync();
            return F.AudioPlayer.Play(clipName, loop, channel);
        }

        public static async ETask<AudioObject> PlayMusicAsync(string resourceName = "MyMusic", bool loop = false)
        {
            await F.InitializeAsync();
            return F.AudioPlayer.PlayMusic(resourceName, loop);
        }

        public static async ETask<AudioObject> PlayEffectAsync(string resourceName = "MyEffect", bool loop = false)
        {
            await F.InitializeAsync();
            return F.AudioPlayer.PlayAudio(resourceName, loop);
        }

        public static bool IsHandlePlaying(AudioObject audio) => audio.IsPlaying();
        public static void Stop(AudioObject audio) => audio.Stop();
        public static int GetClipCount(string clipName) => F.AudioPlayer.GetClipCount(clipName);
        public static bool IsPlaying(string clipName) => F.AudioPlayer.IsPlaying(clipName);

        // StopAll 影响共享播放器；单个业务用返回句柄的 Stop() 结束自己的播放。
        public static void StopAll() => F.AudioPlayer.StopAll();
        public static void StopAll(EAudioType channel) => F.AudioPlayer.StopAll(channel);

        public static bool GetMute(EAudioType channel) => F.AudioPlayer.GetMute(channel);
        public static void SetMute(EAudioType channel, bool mute) => F.AudioPlayer.SetMute(channel, mute);
        public static float GetVolume(EAudioType channel) => F.AudioPlayer.GetVolume(channel);
        public static void SetVolume(EAudioType channel, float volume) => F.AudioPlayer.SetVolume(channel, volume);
    }
}
