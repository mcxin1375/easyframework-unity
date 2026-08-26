/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/11/7
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;

namespace EasyFramework
{
    public class AssetBundleManifest
    {
        public const string FileName = "AssetBundleManifest.json";
        
        public string[] abNames;
        public Dictionary<string, string[]> depDict = new();
        
        public string[] GetAllDependencies(string abName) => depDict.GetValueOrDefault(abName);
    }
}