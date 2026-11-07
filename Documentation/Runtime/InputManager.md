# F.InputManager

`F.InputManager` 的类型是 `IInputManager`，通过 `OnInputEvent` 事件向业务发送输入消息。

```csharp
using EasyFramework;
using UnityEngine;

sealed class InputExample
{
    public void Subscribe() => F.InputManager.OnInputEvent += OnInput;

    private void OnInput(EInputType type, int index, Vector2 value)
    {
        // 根据项目定义的 EInputType 处理输入。
    }

    public void Unsubscribe() => F.InputManager.OnInputEvent -= OnInput;
}
```

事件参数依次为 EInputType（Down、Hover、Up）、输入索引和屏幕坐标。当前使用旧版 UnityEngine.Input，不是 Input System 包；Player Settings 需要启用旧版输入或 Both。Windows / macOS 在无触摸时使用鼠标，鼠标按钮索引为 1001 至 1003；触摸为 fingerId + 1，其他平台当前只采集触摸。组件应在 OnEnable 订阅、OnDisable 解绑，不应依赖跨平台固定索引。

源码：[IInputManager.cs](../../EasyFramework/Runtime/Manager/InputManager/IInputManager.cs)、[InputManager.cs](../../EasyFramework/Runtime/Manager/InputManager/InputManager.cs)。
