/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    [Serializable]
    public class DLCVersionInfo
    {
        public const string FileName = "DLCVersionInfo.json";
        
        public HashFileInfo[] hashFiles;
    }
}
