/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    [Serializable]
    public class DLCBuilderVersion : ToolVersion
    {
        public const string FileName = "DLCBuilderVersion.json";
        
        public DLCVersion dlcVersion;
        public ToolVersion abBuilderVersion = new();
        public ToolVersion dllBuilderVersion = new();
    }
}