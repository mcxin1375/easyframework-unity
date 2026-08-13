/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/11/7
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace EasyFramework
{
    [Serializable]
    public class AssetBundleManifestItem
    {
        public string abName;
        public string[] deps;

    }

    [Serializable]
    public class AssetBundleManifest
    {
        public const string FileName = "AssetBundleManifest.json";
        
        public string[] abNames;
        public AssetBundleManifestItem[] abItems;

        private Dictionary<string, string[]> _depDict;

        public void BuildManifest()
        {
            if (_depDict != null) return;
            _depDict = new Dictionary<string, string[]>();

            foreach (var abName in abItems)
            {
                _depDict.Add(abName.abName, abName.deps);
            }
        }
        public string[] GetAllDependencies(string abName) => _depDict?.GetValueOrDefault(abName);
    }
}