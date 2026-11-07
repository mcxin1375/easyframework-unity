/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using UnityEngine;
using UnityEngine.U2D;

namespace EasyFramework
{
    public interface ISpriteLoader
    {
        Sprite LoadSprite(string spriteName);
        Sprite LoadSprite(string atlasName, string spriteName);
        SpriteAtlas LoadSpriteAtlas(string atlasName);
        void UnloadSpriteAtlas(string atlasName);
        void UnloadAllSpriteAtlas();
    }
}