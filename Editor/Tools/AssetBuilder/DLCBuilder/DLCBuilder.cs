/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class DLCBuilder : ToolBase<DLCBuilder>, IToolEvent<AssetBuilder>
    {
        public override int Order => ToolOrder.DLCBuilder;

        protected override void OnSelfExecute() => DLCBuilderUtility.BuildBySettings(ProjectDataPath);
        
        [MenuItem("EasyFramework/Tools/DLCBuilder - Execute", priority = ToolOrder.DLCBuilder)]
        public static void MenuItem1()
        {
            Instance.Execute();
        }
    }
}