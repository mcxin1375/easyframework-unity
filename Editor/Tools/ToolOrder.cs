/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework.Editor
{
    public static class ToolOrder
    {
        public const int AssetImporter = 1000;
        
        public const int AssetCreator = 2000;
        
        public const int AssetBuilder = 3000;
        public const int HybridCLRBuilder = AssetBuilder + 100;
        public const int AssetBundleBuilder = AssetBuilder + 200;
        public const int DLCBuilder = AssetBuilder + 300;
        
        public const int PlayerBuilder = 4000;
    }
}