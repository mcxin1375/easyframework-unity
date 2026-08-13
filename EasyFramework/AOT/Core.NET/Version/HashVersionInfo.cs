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
    public abstract class HashVersionInfo : ToolVersion
    {
        public HashFileInfo[] hashFiles;

        private Dictionary<string, string> _nameDict;
        
        public void InitNames()
        {
            _nameDict = new();
            if (hashFiles != null)
            {
                foreach (var file in hashFiles) _nameDict.Add(file.fileName, file.hashFileName);
            }
        }

        public string GetHashFileName(string fileName) => _nameDict[fileName];
    }
}