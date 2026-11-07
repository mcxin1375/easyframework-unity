/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace EasyFramework
{
    [Serializable]
    public class DLCVersionInfo
    {
        public const string FileName = "DLCVersionInfo.json";

        public string uid;
        public HashFileInfo[] hashFiles;

        private readonly Dictionary<string, HashFileInfo> _infoDict = new();
        private readonly Dictionary<string, string> _nameDict = new();

        public void RefreshNames()
        {
            _infoDict.Clear();
            _nameDict.Clear();

            if (hashFiles == null) return;
            
            foreach (var info in hashFiles)
            {
                _infoDict[info.resName] = info;
                _nameDict[info.resName] = info.fileName;
            }
        }

        public HashFileInfo GetFileInfo(string resName) => _infoDict.GetValueOrDefault(resName);
        public string GetFileName(string resName) => _nameDict.GetValueOrDefault(resName);

    }
}
