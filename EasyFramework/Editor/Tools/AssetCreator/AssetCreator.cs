/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class AssetCreator : ToolBase<AssetCreator>
    {
        public override int Order => ToolOrder.AssetCreator;

        [MenuItem("EasyFramework/Tools/AssetCreator - Execute", priority = Editor.ToolOrder.AssetCreator)]
        public static void MenuItem()
        {
            Instance.Execute();
            AssetDatabase.Refresh();
        }
    }
}