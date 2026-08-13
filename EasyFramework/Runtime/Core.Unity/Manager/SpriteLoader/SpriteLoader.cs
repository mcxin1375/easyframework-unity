/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace EasyFramework
{
    public class SpriteLoader : Singleton<SpriteLoader>, ISpriteLoader
    {
        public IReadOnlyDictionary<string, SpriteAtlasInfo> AtlasDict => _atlasDict;
        
        private readonly Dictionary<string, SpriteAtlasInfo> _atlasDict = new();

        public SpriteLoader()
        {
            SpriteAtlasManager.atlasRegistered += AtlasRegistered;
            SpriteAtlasManager.atlasRequested += AtlasRequested;
        }

        private void AtlasRequested(string name, Action<SpriteAtlas> action)
        {
            // Debug.Log($"Atlas requested: {name}");
            var spriteAtlas = LoadSpriteAtlas(name);
            action(spriteAtlas);
        }

        private void AtlasRegistered(SpriteAtlas spriteAtlas)
        {
            // Debug.Log($"Atlas AtlasRegistered: {spriteAtlas.name}");
        }

        public Sprite LoadSprite(string spriteName)
        {
            string[] arr = spriteName.Split('_');
            if (arr.Length > 1)
            {
                // string atlasName = $"{EasyFrameworkSettings.Instance.atlasHead}{arr[0]}";
                var atlasName = arr[0];
                return LoadSprite(atlasName, spriteName);
            }
            return null;
        }

        public Sprite LoadSprite(string atlasName, string spriteName)
        {
            if (!_atlasDict.TryGetValue(atlasName, out var info))
            {
                info = new SpriteAtlasInfo(atlasName);
                _atlasDict.Add(atlasName, info);
            }
            return info.LoadSprite(spriteName);
        }

        public SpriteAtlas LoadSpriteAtlas(string atlasName)
        {
            if (!_atlasDict.TryGetValue(atlasName, out var info))
            {
                info = new SpriteAtlasInfo(atlasName);
                _atlasDict.Add(atlasName, info);
            }
            return info.LoadAtlas();
        }

        public void UnloadSpriteAtlas(string atlasName)
        {
            if (_atlasDict.TryGetValue(atlasName, out var info)) info.Unload();
        }

        public void UnloadAllSpriteAtlas()
        {
            foreach (var info in _atlasDict.Values) info.Unload();
        }
    }
}