// using System;
// using System.Threading;
// using UnityEngine.SceneManagement;
//
// namespace EasyFramework
// {
//     public partial class SceneLoader
//     {
//         public class Command : ICommand
//         {
//             public enum ECommandType
//             {
//                 LoadAsync,
//                 UnloadAsync,
//                 UnloadAllAsync,
//             }
//             
//             private SceneLoader _sceneLoader;
//             private ECommandType _commandType;
//             private string _sceneName;
//             private LoadSceneMode _loadSceneMode;
//             private bool _active = true;
//
//             public async EasyTask OnExecuteAsync(CancellationToken token)
//             {
//                 FDebug.Log($"F.SceneSystem.Command OnExecuteAsync. cmdType: {_commandType} name: {_sceneName} mode: {_loadSceneMode} active: {_active}", LogTag.EasyFramework);
//                 
//                 switch (_commandType)
//                 {
//                     case ECommandType.LoadAsync: await LoadAsync();
//                         break;
//                     case ECommandType.UnloadAsync: await UnloadAsync(_sceneName);
//                         break;
//                     case ECommandType.UnloadAllAsync: await UnloadAllAsync();
//                         break;
//                 }
//                 
//                 ObjectPool<Command>.Shared.Return(this);
//             }
//
//             public void OnCancel()
//             {
//                 ObjectPool<Command>.Shared.Return(this);
//             }
//
//             private async EasyTask LoadAsync()
//             {
//                 var componentDict = _sceneLoader._componentDict;
//                 if (!componentDict.TryGetValue(_sceneName, out var component))
//                 {
//                     component = new SceneInfo(_sceneName);
//                     componentDict.Add(_sceneName, component);
//                 }
//                 await component.LoadAsync(_loadSceneMode);
//                 if (_active) F.SceneLoader.SetActive(_sceneName);
//             }
//
//             private EasyTask UnloadAsync(string sceneName)
//             {
//                 var componentDict = _sceneLoader._componentDict;
//                 if (componentDict.TryGetValue(sceneName, out var component))
//                 {
//                     return component.UnloadAsync();
//                 }
//                 return EasyTask.CompletedTask;
//             }
//
//             private async EasyTask UnloadAllAsync()
//             {
//                 var keys = _sceneLoader._componentDict.Keys;
//                 foreach (var sceneName in keys)
//                     await UnloadAsync(sceneName);
//             }
//             
//             public static Command Create(SceneLoader loader, ECommandType commandType) => Create(loader, commandType, string.Empty, LoadSceneMode.Additive, false);
//             public static Command Create(SceneLoader loader, ECommandType commandType, string sceneName) => Create(loader, commandType, sceneName, LoadSceneMode.Additive, false);
//             public static Command Create(SceneLoader loader, ECommandType commandType, string sceneName, LoadSceneMode loadSceneMode, bool active)
//             {
//                 var source = ObjectPool<Command>.Shared.Rent();
//                 source._sceneLoader = loader;
//                 source._commandType = commandType;
//                 source._sceneName = sceneName;
//                 source._loadSceneMode = loadSceneMode;
//                 source._active = active;
//                 return source;
//             }
//         }
//
//     }
// }