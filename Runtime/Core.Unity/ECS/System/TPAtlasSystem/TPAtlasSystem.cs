// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2018/3/1
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using System.Collections.Generic;
// using UnityEngine;
//
// namespace EasyFramework
// {
//     public class TPAtlasSystem : FSystem
//     {
//         internal IReadOnlyDictionary<string, TPAtlasComponent> AtlasDict => _atlasDict;
//         private readonly Dictionary<string, TPAtlasComponent> _atlasDict = new();
//
//         public Sprite LoadSprite(string spriteName)
//         {
//             // string[] arr = spriteName.Split('_');
//             // if (arr.Length > 1)
//             // {
//             //     string atlasName = $"{EasyFrameworkSettings.Instance.tpAtlasPrefix}{arr[0]}";
//             //     return LoadSprite(atlasName, spriteName);
//             // }
//             return null;
//         }
//
//         public Sprite LoadSprite(string atlasName, string spriteName)
//         {
//             if (!_atlasDict.TryGetValue(atlasName, out var component))
//             {
//                 component = new TPAtlasComponent(atlasName);
//                 _atlasDict.Add(atlasName, component);
//                 component.LoadAtlas();
//             }
//             return component.LoadSprite(spriteName);
//         }
//
//         public void PreLoadAtlas(string atlasName, bool neverUnload = false)
//         {
//             if (!_atlasDict.TryGetValue(atlasName, out var component))
//             {
//                 component = new TPAtlasComponent(atlasName);
//                 _atlasDict.Add(atlasName, component);
//             }
//             component.NeverUnload = neverUnload;
//             component.LoadAtlas();
//         }
//
//         public void UnloadAtlas(string atlasName)
//         {
//             if (_atlasDict.TryGetValue(atlasName, out var component))
//             {
//                 component.NeverUnload = false;
//                 component.Unload();
//             }
//         }
//
//         public void UnloadAtlasWithoutNeverUnload()
//         {
//             foreach (var value in _atlasDict.Values)
//             {
//                 if (value.NeverUnload) continue;
//                 value.Unload();
//             }
//         }
//         public async EasyTask UnloadAtlasWithoutNeverUnloadAsync()
//         {
//             foreach (var value in _atlasDict.Values)
//             {
//                 if (value.NeverUnload) continue;
//                 await value.UnloadAsync();
//             }
//         }
//     }
// }