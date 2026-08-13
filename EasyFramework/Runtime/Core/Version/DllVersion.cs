/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    [Serializable]
    public class DllVersion
    {
        public ToolVersion dllBuildVersion;
        
        public string[] dlls;
        public string[] stripDlls;
    }
}