using System;
using UnityEngine;

namespace EasyFramework.Samples.Modules
{
    // 基础生命周期：首次创建、每次进入/刷新、激活、帧更新和退出。
    public sealed class TemplateController : Controller<TemplateController>
    {
        public int Updates { get; private set; }
        public int LateUpdates { get; private set; }
        protected override void OnCreate() => Debug.Log("创建 Controller 实例");
        protected override async ETask OnPreEnterAsync() => await ETask.DelayFrame(1);
        protected override void OnEnter() => Debug.Log("进入业务流程");
        protected override ETask OnEnterAsync() => ETask.CompletedTask;
        protected override void OnAfterEnter() => Debug.Log("进入完成阶段");
        protected override ETask OnAfterEnterAsync() => ETask.CompletedTask;
        protected override void OnSetActive(bool active) => Debug.Log($"Controller 激活：{active}");
        protected override void OnUpdate() => Updates++;
        protected override void OnLateUpdate() => LateUpdates++;
        protected override void OnExit() => Debug.Log("退出业务流程");
        protected override ETask OnExitAsync() => ETask.CompletedTask;
    }

    public static class ControllerExample
    {
        // Unity 主线程顺序 await EnterAsync()/ExitAsync()；退出结束本轮业务，管理器仍缓存实例供复用。
        public static async ETask<TemplateController> EnterAsync()
        {
            await F.InitializeAsync();
            return await TemplateController.Instance.EnterAsync();
        }

        public static ETask ExitAsync() => TemplateController.Instance.ExitAsync();

        // 只有实例入口发送完成事件；重复 Enter 刷新同一实例，不先 Exit、不增加重复栈项。
        public static async ETask RefreshWithCompletionEventsAsync()
        {
            await F.InitializeAsync();
            var controller = TemplateController.Instance;
            Action entered = () => FDebug.Log("Instance OnEnterEvent");
            Action exited = () => FDebug.Log("Instance OnExitEvent");
            controller.OnEnterEvent += entered;
            controller.OnExitEvent += exited;
            try
            {
                await controller.EnterAsync();
                await controller.EnterAsync();
                await controller.ExitAsync();
            }
            finally
            {
                controller.OnEnterEvent -= entered;
                controller.OnExitEvent -= exited;
                if (controller.IsEnter) await controller.ExitAsync();
            }
        }
    }

    public readonly struct ControllerArgs
    {
        public readonly int Id;
        public ControllerArgs(int id) => Id = id;
    }

    // ITParams 的 T1 / SetParams 成员由包内 Roslyn 生成器补齐。
    public partial class OneParamController : Controller<OneParamController>, ITParams<ControllerArgs>
    {
        public void SetParamsDefault() => T1RW = new ControllerArgs(0);
        protected override void OnEnter() => FDebug.Log($"Enter id={T1.Id}");
    }

    public partial class TwoParamController : Controller<TwoParamController>, ITParams<ControllerArgs, string>
    {
        protected override void OnEnter() => FDebug.Log($"Enter id={T1.Id}, mode={T2}");
    }

    public partial class ThreeParamController : Controller<ThreeParamController>, ITParams<ControllerArgs, string, bool>
    {
        protected override void OnEnter() => FDebug.Log($"Enter id={T1.Id}, mode={T2}, enabled={T3}");
    }

    public static class ControllerParamsExample
    {
        public static async ETask EnterOneAsync(int id)
        {
            await F.InitializeAsync();
            await OneParamController.Instance.EnterAsync(new ControllerArgs(id));
            await OneParamController.Instance.ExitAsync();
            await F.ControllerManager.EnterAsync<OneParamController, ControllerArgs>(new ControllerArgs(id));
            await F.ControllerManager.ExitAsync<OneParamController>();
        }

        public static async ETask EnterTwoAsync(int id, string mode)
        {
            await F.InitializeAsync();
            await TwoParamController.Instance.EnterAsync(new ControllerArgs(id), mode);
            await TwoParamController.Instance.ExitAsync();
            await F.ControllerManager.EnterAsync<TwoParamController, ControllerArgs, string>(new ControllerArgs(id), mode);
            await F.ControllerManager.ExitAsync<TwoParamController>();
        }

        public static async ETask EnterThreeAsync(int id, string mode, bool enabled)
        {
            await F.InitializeAsync();
            await ThreeParamController.Instance.EnterAsync(new ControllerArgs(id), mode, enabled);
            await ThreeParamController.Instance.ExitAsync();
            await F.ControllerManager.EnterAsync<ThreeParamController, ControllerArgs, string, bool>(new ControllerArgs(id), mode, enabled);
            await F.ControllerManager.ExitAsync<ThreeParamController>();
        }
    }

    public readonly struct ControllerSignal { }

    // Controller<T> 会发现并管理实现 IControllerComponent 的字段。
    public sealed class ComponentControllerExample : Controller<ComponentControllerExample>
    {
        private readonly TraceControllerComponent _trace = new();

        protected override void OnAddListeners() => F.Event.Add<ControllerSignal>(OnSignal);
        protected override void OnRemoveListeners() => F.Event.Remove<ControllerSignal>(OnSignal);
        protected override void OnEnter() => F.Event.Invoke(new ControllerSignal());
        protected override void OnExit() => FDebug.Log("Controller exit");

        private void OnSignal(ControllerSignal signal) => FDebug.Log("Controller listener received signal");
    }

    public sealed class TraceControllerComponent : ControllerComponent
    {
        public int Updates { get; private set; }
        public int LateUpdates { get; private set; }
        protected override void OnCreate() => FDebug.Log("Component created");
        protected override void OnPreEnter() => FDebug.Log("Component before-enter");
        protected override void OnEnter() => FDebug.Log("Component entered");
        protected override void OnAfterEnter() => FDebug.Log("Component after-enter");
        protected override void OnSetActive(bool active) => FDebug.Log($"Component active={active}");
        protected override void OnUpdate() => Updates++;
        protected override void OnLateUpdate() => LateUpdates++;
        protected override void OnExit() => FDebug.Log("Component exited");
        protected override void OnDestroy() => FDebug.Log("Component destroyed");
    }

    public static class ControllerComponentExample
    {
        public static async ETask RunAsync()
        {
            await F.InitializeAsync();
            await ComponentControllerExample.Instance.EnterAsync();
            await ComponentControllerExample.Instance.ExitAsync();
        }
    }

    public static class ControllerManagerExample
    {
        // 管理器入口不发送 Controller<T> 的完成事件；栈和参数仍由管理器统一管理。
        public static async ETask InspectAndExitAsync()
        {
            await F.InitializeAsync();
            var controller = await F.ControllerManager.EnterAsync<TemplateController>();
            IController found = F.ControllerManager.GetController<TemplateController>();
            IController top = F.ControllerManager.TopController;
            bool entered = F.ControllerManager.HasEnter(controller);
            bool active = F.ControllerManager.HasActive(controller);
            int stackCount = F.ControllerManager.EnterList.Count;
            FDebug.Log($"Found={found}, Top={top}, Entered={entered}, Active={active}, Count={stackCount}");
            await F.ControllerManager.ExitAsync(controller);
        }

        // 全局操作：只在业务明确要退出整个进入批次时调用，不能用来清理单个消费者。
        public static async ETask ExitAllAsync()
        {
            await F.InitializeAsync();
            await F.ControllerManager.ExitAllAsync();
        }
    }

    // 自定义 IController 直接查询管理器状态，自行处理组件、监听和业务清理。
    public sealed class CustomControllerExample : IController
    {
        public bool IsEnter => F.ControllerManager.HasEnter(this);
        public bool IsActive => F.ControllerManager.HasActive(this);
        public bool UpdateEnabled => IsEnter && IsActive;

        public void OnCreate() => FDebug.Log("Custom controller created");
        public ETask OnPreEnterAsync() => ETask.CompletedTask;
        public ETask OnEnterAsync()
        {
            FDebug.Log("Custom controller entered");
            return ETask.CompletedTask;
        }
        public ETask OnExitAsync()
        {
            FDebug.Log("Custom controller exited");
            return ETask.CompletedTask;
        }
        public void OnSetActive(bool active) => FDebug.Log($"Custom active={active}");
        public void OnUpdate() { }
        public void OnLateUpdate() { }
        public void OnDestroy() => FDebug.Log("Custom controller destroyed");
    }

    public static class CustomControllerUseExample
    {
        public static async ETask RunAsync()
        {
            await F.InitializeAsync();
            var controller = await F.ControllerManager.EnterAsync<CustomControllerExample>();
            await F.ControllerManager.ExitAsync(controller);
        }
    }
}
