
using System;

namespace EasyFramework
{
    [Serializable]
    public class EasyFrameworkConfig : SingletonJson<EasyFrameworkConfig>
    {
        public string mainResUid;
        public string dlcVersion;
        public string dlcVersionUid;
    }
}