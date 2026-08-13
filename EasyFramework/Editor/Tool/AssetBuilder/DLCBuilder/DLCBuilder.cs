/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public partial class DLCBuilder : SingletonTool<DLCBuilder>
    {
        
        [MenuItem("EasyFramework/Tools/DLCBuilder - Execute", priority = ToolOrder.DLCBuilder)]
        public static void MenuItem1()
        {
            Instance.Execute();
        }
        
        class DLCBuilderTrigger : IToolEvent<AssetBuilder>, IToolEvent<DLCBuilder>
        {
            int IToolEvent<AssetBuilder>.Order => 1;
            void IToolEvent<AssetBuilder>.OnExecute()
            {
                Instance.Execute();
            }


            void IToolEvent<DLCBuilder>.OnExecute()
            {
                Instance.BuildBySettings();
            }
        }
    }
}