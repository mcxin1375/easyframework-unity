/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class ToolImporter : ToolBase<ToolImporter>
    {
        
        [MenuItem("EasyFramework/Tools/ToolImporter - Execute", priority = ToolOrder.ToolImporter)]
        public static void MenuItem()
        {
            Instance.Execute();
            AssetDatabase.Refresh();
        }
    }
}