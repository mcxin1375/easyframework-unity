# F.InputManager

`F.InputManager` 的类型是 `IInputManager`，通过 `OnInputEvent` 事件向业务发送输入消息。

```csharp
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

事件参数依次为 `EInputType`、输入索引 `int` 和输入值 `Vector2`。输入枚举和具体设备映射由当前项目实现决定，业务不应把某个数值索引当成跨项目契约。

源码：[IInputManager.cs](../../Runtime/Manager/InputManager/IInputManager.cs)、[InputManager.cs](../../Runtime/Manager/InputManager/InputManager.cs)。
