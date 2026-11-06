using System;
using UnityEngine;

namespace EasyFramework.Samples.Modules
{
    public readonly struct ModuleMessage
    {
        public readonly string Text;
        public ModuleMessage(string text) => Text = text;
    }

    // 同一线程注册/派发/解绑；对象和委托两组先后执行，同步和异步通道完全独立。
    // 派发使用快照，回调内增删订阅从下一次派发生效；回调异常由框架记录并继续。
    public sealed class EventExample : IEvent<ModuleMessage>, IAsyncEvent<ModuleMessage>
    {
        public static async ETask RunAsync()
        {
            await F.InitializeAsync();
            RunSynchronous();
            await RunAsynchronousAsync();
        }

        public static void RunSynchronous()
        {
            var handler = new EventExample();
            Action<ModuleMessage> action = args => Debug.Log($"同步委托：{args.Text}");
            F.Event.Add<ModuleMessage>(handler);
            F.Event.Add(action);
            try { F.Event.Invoke(new ModuleMessage("同步消息")); }
            finally
            {
                F.Event.Remove<ModuleMessage>(handler);
                F.Event.Remove(action); // 解绑原委托，重新写 lambda 不能替代。
            }
        }

        public static async ETask RunAsynchronousAsync()
        {
            var handler = new EventExample();
            Func<ModuleMessage, ETask> action = OnDelegateAsync;
            F.Event.AddAsync<ModuleMessage>(handler);
            F.Event.AddAsync(action);
            try { await F.Event.InvokeAsync(new ModuleMessage("异步消息")); }
            finally
            {
                F.Event.RemoveAsync<ModuleMessage>(handler);
                F.Event.RemoveAsync(action);
            }
        }

        public void Execute(in ModuleMessage args) => Debug.Log($"同步对象：{args.Text}");

        public async ETask ExecuteAsync(ModuleMessage args)
        {
            await ETask.DelayFrame(1);
            Debug.Log($"异步对象：{args.Text}");
        }

        private static async ETask OnDelegateAsync(ModuleMessage args)
        {
            await ETask.DelayFrame(1);
            Debug.Log($"异步委托：{args.Text}");
        }
    }
}
