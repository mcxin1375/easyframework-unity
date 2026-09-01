# F.ShaderLoader

`F.ShaderLoader` 的类型是 `IShaderLoader`，负责加载 Shader 所在资源并按名称查询 Shader。

| API | 说明 |
| --- | --- |
| `Load(string abName)` | 同步加载指定资源包 |
| `LoadAsync(string abName)` | 异步加载指定资源包 |
| `GetShader(string shaderName)` | 按名称获取 Shader |

先加载资源包，再查询 Shader。加载失败或资源未配置时 `GetShader` 可能返回空，调用方应处理该情况。

源码：[IShaderLoader.cs](../../Runtime/Manager/ShaderLoader/IShaderLoader.cs)。
