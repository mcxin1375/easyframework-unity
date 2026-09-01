# F.SpriteLoader

`F.SpriteLoader` 的类型是 `ISpriteLoader`，负责从 SpriteAtlas 获取 Sprite 或 Atlas，并提供卸载操作。

| API | 说明 |
| --- | --- |
| `LoadSprite(string spriteName)` | 按 Sprite 名加载 |
| `LoadSprite(string atlasName, string spriteName)` | 从指定 Atlas 加载 Sprite |
| `LoadSpriteAtlas(string atlasName)` | 加载 SpriteAtlas |
| `UnloadSpriteAtlas(string atlasName)` | 卸载指定 Atlas |
| `UnloadAllSpriteAtlas()` | 卸载全部 Atlas |

返回的 Unity 对象依赖资源加载器的持有关系。使用完成后按 Atlas 粒度释放，不要在卸载后继续保存并使用 Sprite 引用。

源码：[ISpriteLoader.cs](../../Runtime/Manager/SpriteLoader/ISpriteLoader.cs)。
