
using System;

namespace EasyFramework
{
    [Serializable]
    public class EasyFrameworkConfig : SingletonConfig<EasyFrameworkConfig>
    {
        public string mainResUid;
        public string dlcVersion;
        public string dlcVersionInfoUid;
    }
}