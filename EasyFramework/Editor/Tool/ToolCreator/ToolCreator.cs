/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class ToolCreator : ToolBase<ToolCreator>
    {
        
        [MenuItem("EasyFramework/Tools/ToolCreator - Execute", priority = ToolOrder.ToolCreator)]
        public static void MenuItem()
        {
            Instance.Execute();
            AssetDatabase.Refresh();
        }
    }
}