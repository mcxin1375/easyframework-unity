/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public partial class DLCBuilder : ToolBase<DLCBuilder>
    {
        
        [MenuItem("EasyFramework/Tools/DLCBuilder - Execute", priority = ToolOrder.DLCBuilder)]
        public static void MenuItem1()
        {
            Instance.Execute();
        }
    }
}