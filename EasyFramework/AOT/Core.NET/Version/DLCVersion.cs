/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    [Serializable]
    public class DLCVersion
    {
        public const string FileName = "DLCVersion.json";
        
        public int mainVersion;
        public string versionName;
        public string versionUid;
    }
}