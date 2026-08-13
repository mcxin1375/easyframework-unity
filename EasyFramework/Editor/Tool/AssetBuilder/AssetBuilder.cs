/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class AssetBuilder : SingletonTool<AssetBuilder>
    {
        
        [MenuItem("EasyFramework/Tools/AssetBuilder - Execute", priority = ToolOrder.AssetBuilder)]
        public static void MenuItem()
        {
            Instance.Execute();
            AssetDatabase.Refresh();
        }
    }
}