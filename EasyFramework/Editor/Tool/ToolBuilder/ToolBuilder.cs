/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class ToolBuilder : ToolBase<ToolBuilder>
    {
        
        [MenuItem("EasyFramework/Tools/ToolBuilder - Execute", priority = ToolOrder.ToolBuilder)]
        public static void MenuItem()
        {
            Instance.Execute();
            AssetDatabase.Refresh();
        }
    }
}