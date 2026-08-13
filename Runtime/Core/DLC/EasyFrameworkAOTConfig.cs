
using System;
using System.Collections.Generic;

namespace EasyFramework
{
    [Serializable]
    public class EasyFrameworkAOTConfig : SingletonJson<EasyFrameworkAOTConfig>
    {
        public string PlayerVersionUid;
        public string MainResUid;
        public DLCVersion DLCVersion = new();
        public List<string> PackageList = new List<string>();
    }
}