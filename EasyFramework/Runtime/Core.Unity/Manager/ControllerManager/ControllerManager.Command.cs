/*----------------------------------------------------------------
// author??Cookie(mcx)
// date??2023/12/5
// describe??
//----------------------------------------------------------------*/

using System.Threading;

namespace EasyFramework
{
    public enum EControllerEnter
    {
        Single,
        Additive,
        AdditiveBackground
    }

    internal partial class ControllerManager
    {
        private sealed class Command : ICommand
        {
            public enum ECommandType
            {
                EnterAsync,
                SetActiveTrueAsync,
                SetActiveFalseAsync,
                ExitAsync,
                ExitAllAsync,
            }

            private ControllerManager _controllerManager;
            private ECommandType _commandType;
            private IController _controller;
            private EControllerEnter _eControllerEnter;

            public async ETask OnExecuteAsync(CancellationToken token)
            {
                switch (_commandType)
                {
                    case ECommandType.EnterAsync: await EnterAsync(_controller);
                        break;
                    case ECommandType.SetActiveTrueAsync: await SetActiveAsync(_controller, true);
                        break;
                    case ECommandType.SetActiveFalseAsync: await SetActiveAsync(_controller, false);
                        break;
                    case ECommandType.ExitAsync: await ExitAsync(_controller);
                        break;
                    case ECommandType.ExitAllAsync: await ExitAllAsync();
                        break;
                }
                
                ObjectPool<Command>.Shared.Return(this);
            }

            public void OnCancel()
            {
                ObjectPool<Command>.Shared.Return(this);
            }

            private async ETask EnterAsync(IController controller)
            {
                if (controller == null) return;
                
                await controller.BeforeEnterAsync(_eControllerEnter);
                
                var enterList = _controllerManager._enterList;
                switch (_eControllerEnter)
                {
                    case EControllerEnter.Single:

                        for (int i = enterList.Count - 1; i >= 0; i--)
                        {
                            var c = enterList[i];
                            if (c == controller) continue;

                            enterList.RemoveAt(i);
                            _controllerManager._needRefresh = true;
                            await c.ExitAsync();
                            
                            _controllerManager.OnExit?.Invoke(c);
                        }

                        if (!enterList.Contains(controller))
                        {
                            enterList.Add(controller);
                            _controllerManager._needRefresh = true;
                        }
                        break;
                    case EControllerEnter.Additive:

                        var preCtr = enterList.Count > 0 ? enterList[^1] : null;
                        if (preCtr != null && preCtr != controller)
                        {
                            await preCtr.SetActiveAsync(false);
                        }
                        if (enterList.Contains(controller)) enterList.Remove(controller);
                        enterList.Add(controller);
                        _controllerManager._needRefresh = true;
                        
                        break;
                    case EControllerEnter.AdditiveBackground:
                        
                        if (!enterList.Contains(controller)) 
                        {
                            enterList.Insert(0, controller);
                            _controllerManager._needRefresh = true;
                        }
                        
                        break;
                }

                await controller.EnterAsync(_eControllerEnter);
                
                _controllerManager.OnEnter?.Invoke(controller);
            }

            private async ETask SetActiveAsync(IController controller, bool isActive)
            {
                if (controller == null) return;

                var enterList = _controllerManager._enterList;
                if (!enterList.Contains(controller)) return;
                
                if (enterList.Count == 1)
                {
                    await controller.SetActiveAsync(isActive);
                    return;
                }
                
                enterList.Remove(controller);
                var topCtr = enterList[^1];

                if (isActive)
                {                    
                    enterList.Add(controller);
                }
                else
                {
                    enterList.Insert(enterList.Count - 1, controller);
                }

                await topCtr.SetActiveAsync(!isActive);
                await controller.SetActiveAsync(isActive);
            }

            private async ETask ExitAsync(IController controller)
            {
                if (controller == null) return;
                
                var enterList = _controllerManager._enterList;
                if (!enterList.Contains(controller)) return;

                enterList.Remove(controller);
                _controllerManager._needRefresh = true;

                var topCtr = enterList.Count > 0 ? enterList[^1] : null;
                if (topCtr != null)
                {
                    await topCtr.SetActiveAsync(true);
                }

                await controller.ExitAsync();
                
                _controllerManager.OnExit?.Invoke(controller);
            }

            private async ETask ExitAllAsync()
            {
                var arr = _controllerManager._enterList.ToArray();
                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    var c = arr[i];
                    await ExitAsync(c);
                }
            }

            public static Command SetActiveAsync(ControllerManager controllerManager, IController controller, bool active)
            {
                var source = ObjectPool<Command>.Shared.Rent();
                source._controllerManager = controllerManager;
                source._commandType = active ? ECommandType.SetActiveTrueAsync : ECommandType.SetActiveFalseAsync;
                source._controller = controller;
                return source;
            }
            public static Command Create(ControllerManager controllerManager, ECommandType commandType) => Create(controllerManager, commandType, null, EControllerEnter.Additive);
            public static Command Create(ControllerManager controllerManager, ECommandType commandType, IController controller) => Create(controllerManager, commandType, controller, EControllerEnter.Additive);
            public static Command Create(ControllerManager controllerManager, ECommandType commandType, IController controller, EControllerEnter eControllerEnter)
            {
                var source = ObjectPool<Command>.Shared.Rent();
                source._controllerManager = controllerManager;
                source._commandType = commandType;
                source._controller = controller;
                source._eControllerEnter = eControllerEnter;
                return source;
            }
        }

    }
}