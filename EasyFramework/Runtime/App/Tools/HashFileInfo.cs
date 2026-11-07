/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    [Serializable]
    public class HashFileInfo
    {
        public string resName; // 资源名带后缀
        public string fileName; // 文件名后缀
        public long length;
    }
}