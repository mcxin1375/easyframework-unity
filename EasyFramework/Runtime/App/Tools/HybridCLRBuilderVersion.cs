/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    [Serializable]
    public class HybridCLRBuilderVersion : ToolVersion
    {
        public const string FileName = "HybridCLRBuilderVersion.json";
        
        public string[] stripDlls;
        public string[] allDlls;
        public string[] loadDlls;
    }
}