using UnityEngine;
using UnityEngine.U2D;

namespace EasyFramework.Samples.Modules
{
    // 先等待初始化并准备资源构建目录中的 MyAtlas / MyIcon；释放模板要求业务独占图集。
    public static class SpriteExample
    {
        public static async ETask<Sprite> LoadAsync(string atlasName = "MyAtlas", string spriteName = "MyIcon")
        {
            await F.InitializeAsync();
            return F.SpriteLoader.LoadSprite(atlasName, spriteName);
        }

        // 此重载按框架命名约定从 spriteName 前缀推导图集名。
        public static Sprite Load(string spriteName) => F.SpriteLoader.LoadSprite(spriteName);
        public static Sprite Load(string atlasName, string spriteName) => F.SpriteLoader.LoadSprite(atlasName, spriteName);
        public static SpriteAtlas LoadAtlas(string atlasName) => F.SpriteLoader.LoadSpriteAtlas(atlasName);
        public static void UnloadAtlas(string atlasName) => F.SpriteLoader.UnloadSpriteAtlas(atlasName);

        // 先清除所有 Renderer / UI 引用，再销毁 GetSprite 返回的拷贝并按图集释放。
        public static void Release(Sprite sprite, string atlasName = "MyAtlas")
        {
            if (sprite != null) Object.Destroy(sprite);
            F.SpriteLoader.UnloadSpriteAtlas(atlasName);
        }

        // 全局卸载所有图集；仅应用级清理时调用，勿用于单个图集释放。
        public static void UnloadAllAtlases() => F.SpriteLoader.UnloadAllSpriteAtlas();
    }
}
