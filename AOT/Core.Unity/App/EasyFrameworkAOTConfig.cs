
using System;

namespace EasyFramework
{
    [Serializable]
    public class EasyFrameworkAOTConfig : SingletonJson<EasyFrameworkAOTConfig>
    {
        public string mainResUid;
        public string dlcVersion;
        public string dlcVersionUid;
    }
}