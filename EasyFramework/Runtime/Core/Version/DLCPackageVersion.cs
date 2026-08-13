/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    [Serializable]
    public class DLCPackageVersion
    {
        public int dlcVersion;
        public string packageName;
        public string packageFileName;
        public long listResSize;
        public long zipResSize;
        public ResFileInfo[] listArray;
        public ResFileInfo[] zipArray;
    }
}