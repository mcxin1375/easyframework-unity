/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class AssetBuilder : ToolBase<AssetBuilder>
    {
        public override int Order => ToolOrder.AssetBuilder;
        
        [MenuItem("EasyFramework/Tools/AssetBuilder - Execute", priority = Editor.ToolOrder.AssetBuilder)]
        public static void MenuItem()
        {
            Instance.Execute();
            AssetDatabase.Refresh();
        }
    }
}