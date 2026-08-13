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
        
        public string[] AssetBundles;
        public Dictionary<string, string[]> Dependencies = new();

        public string[] GetAllDependencies(string abName) => Dependencies.GetValueOrDefault(abName);
    }
}