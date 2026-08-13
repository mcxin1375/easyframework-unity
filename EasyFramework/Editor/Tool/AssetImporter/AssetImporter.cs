/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class AssetImporter : SingletonTool<AssetImporter>
    {
        
        [MenuItem("EasyFramework/Tools/AssetImporter - Execute", priority = ToolOrder.AssetImporter)]
        public static void MenuItem()
        {
            Instance.Execute();
            AssetDatabase.Refresh();
        }
    }
}