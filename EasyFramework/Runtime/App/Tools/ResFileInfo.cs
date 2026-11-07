/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    [Serializable]
    public class ResFileInfo
    {
        public string name;
        public long length;
        public long writeTime;
        public uint crc32;

        public bool IsMatchCRC32(ResFileInfo target)
        {
            return target.length == length && target.crc32 == crc32;
        }
    }
    [Serializable]
    public class ResZipInfo
    {
        public int version;
        public string name;
        public string md5;
        public long length;
        public long unzipSize;
        public int fileNum;
    }
}