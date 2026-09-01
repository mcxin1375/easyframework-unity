# F.AudioPlayer

`F.AudioPlayer` 的类型是 `IAudioPlayer`，提供音效、音乐播放和按 `EAudioType` 频道控制。

| API | 说明 |
| --- | --- |
| `AudioListener` | 当前音频监听器 |
| `PlayAudio` | 播放音效，支持循环 |
| `PlayMusic` | 播放音乐，支持循环 |
| `Play` | 按频道播放指定音频 |
| `GetClipCount` | 查询指定音频当前播放数量 |
| `IsPlaying` | 查询指定音频是否播放中 |
| `StopAll` | 停止全部音频，或停止指定频道 |
| `GetMute` / `SetMute` | 查询或设置频道静音 |
| `GetVolume` / `SetVolume` | 查询或设置频道音量 |

```csharp
AudioObject music = F.AudioPlayer.PlayMusic("Bgm", true);
// 需要单独停止时使用返回的 AudioObject。
music.Stop();
```

音频资源名必须符合项目资源配置。播放返回的 `AudioObject` 是值类型句柄，停止前应确认播放对象仍属于当前音频播放器生命周期。

源码：[IAudioPlayer.cs](../../Runtime/Manager/AudioPlayer/IAudioPlayer.cs)、[AudioObject.cs](../../Runtime/Manager/AudioPlayer/AudioObject.cs)。
