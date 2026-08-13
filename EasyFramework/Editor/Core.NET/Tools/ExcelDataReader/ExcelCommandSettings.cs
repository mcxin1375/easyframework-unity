
using System;

namespace EasyFramework.Editor
{
    [Serializable]
    public class ExcelCommandSettings
    {
        public string namespaceName = "Game";
        public string prefixName;
        public string suffixName;
        public string dataPath;
        public string outputDataFilePath;
        public string outputScriptPath;
        public string svnVersionPath;
    }
}
