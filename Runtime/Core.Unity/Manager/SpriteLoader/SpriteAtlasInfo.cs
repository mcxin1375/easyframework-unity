using UnityEngine;
using UnityEngine.U2D;

namespace EasyFramework
{
    public class SpriteAtlasInfo : IResRequest
    {
        public bool Alive { get; private set; }
        public string AtlasName { get; }

        private SpriteAtlas _spriteAtlas;

        internal SpriteAtlasInfo(string atlasName)
        {
            AtlasName = atlasName;
        }

        public Sprite LoadSprite(string spriteName) => LoadAtlas()?.GetSprite(spriteName);
        internal SpriteAtlas LoadAtlas()
        {
            if (_spriteAtlas == null)
            {
                FDebug.Log($"SpriteAtlas.LoadAtlas: {AtlasName}");
                _spriteAtlas = F.ResLoader.LoadAsset<SpriteAtlas>(AtlasName, this);
                Alive = _spriteAtlas != null;
            }

            return _spriteAtlas;
        }
        
        internal void Unload()
        {
            if (_spriteAtlas == null) return;
            
            FDebug.Log($"SpriteAtlas.Unload: {AtlasName}");
            
            _spriteAtlas = null;
            Alive = false;
        }
    }
}