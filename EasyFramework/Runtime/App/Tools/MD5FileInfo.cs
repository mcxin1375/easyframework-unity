/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    [Serializable]
    public class MD5FileInfo
    {
        public string fileName;
        public string md5;
        public long length;
        public long writeTime;
    }
}