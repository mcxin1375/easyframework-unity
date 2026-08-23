/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public partial class DLCBuilder : ToolBase<DLCBuilder>, IToolEvent<AssetBuilder>
    {
        public override int Order => ToolOrder.DLCBuilder;

        protected override void OnToolExecute() => BuildBySettings();
        
        [MenuItem("EasyFramework/Tools/DLCBuilder - Execute", priority = ToolOrder.DLCBuilder)]
        public static void MenuItem1()
        {
            Instance.Execute();
        }
    }
}