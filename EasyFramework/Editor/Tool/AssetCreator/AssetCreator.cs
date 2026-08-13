/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class AssetCreator : SingletonTool<AssetCreator>
    {
        
        [MenuItem("EasyFramework/Tools/AssetCreator - Execute", priority = ToolOrder.AssetCreator)]
        public static void MenuItem()
        {
            Instance.Execute();
            AssetDatabase.Refresh();
        }
    }
}