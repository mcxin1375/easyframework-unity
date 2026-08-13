/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

#if EF_HYBRIDCLR

using UnityEditor;

namespace EasyFramework.Editor
{
    public partial class HybridCLRBuilder : SingletonTool<HybridCLRBuilder>
    {
        
        [MenuItem("EasyFramework/Tools/HybridCLRBuilder - Execute", priority = ToolOrder.HybridCLRBuilder)]
        public static void MenuItem1() => Instance.Execute();
        [MenuItem("EasyFramework/Tools/HybridCLRBuilder - Execute (HybridCLR-CompileDllActiveBuildTarget)", priority = ToolOrder.HybridCLRBuilder)]
        public static void MenuItem2()
        {
            HybridCLR.Editor.Commands.CompileDllCommand.CompileDllActiveBuildTarget();
            Instance.Execute();
        }
        [MenuItem("EasyFramework/Tools/HybridCLRBuilder - Execute (HybridCLR-GenerateAll)", priority = ToolOrder.HybridCLRBuilder)]
        public static void MenuItem3()
        {
            HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
            Instance.Execute();
        }
        

        class HybridCLRBuilderTrigger : IToolEvent<AssetBuilder>, IToolEvent<HybridCLRBuilder>
        {
            void IToolEvent<AssetBuilder>.OnExecute()
            {
                Instance.Execute();
            }
            void IToolEvent<HybridCLRBuilder>.OnExecute()
            {
                Instance.BuildSettings();
            }
        }
    }
}

#endif