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
        public string fileName; // 原资源名带后缀
        public string hashFileName; // 资源名后缀
        public long length;
    }
}