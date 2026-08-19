/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class ToolPublisher : ToolBase<ToolPublisher>
    {
        
        [MenuItem("EasyFramework/Tools/ToolPublisher - Execute", priority = ToolOrder.ToolPublisher)]
        public static void MenuItem()
        {
            Instance.Execute();
            AssetDatabase.Refresh();
        }
    }
}