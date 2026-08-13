// using System.Collections.Generic;
// using UnityEngine;
//
// namespace EasyFramework
// {
//     internal class TPAtlasComponent : IAssetBundleHandler
//     {
//         public bool Alive { get; private set; }
//         public readonly string AtlasName;
//         public bool NeverUnload;
//
//         private readonly Dictionary<string, Sprite> _spriteDict = new();
//
//         internal TPAtlasComponent(string atlasName)
//         {
//             AtlasName = atlasName;
//         }
//
//         internal Sprite LoadSprite(string spriteName) => _spriteDict.GetValueOrDefault(spriteName);
//         internal void LoadAtlas()
//         {
//             Alive = true;
//             Sprite[] sprites = F.ResLoader.LoadAllAssets<Sprite>(AtlasName, this);
//             if (sprites?.Length > 0)
//             {
//                 foreach (Sprite s in sprites)
//                 {
//                     if (!_spriteDict.TryAdd(s.name, s))
//                     {
//                         Debug.LogWarning($"Init atlas sprite repeated! sprite name: {s.name}.");
//                     }
//                 }
//             }
//         }
//
//         internal void Unload()
//         {
//             Alive = false;
//             _spriteDict.Clear();
//             F.ResLoader.Unload(AtlasName, this);
//         }
//     }
// }