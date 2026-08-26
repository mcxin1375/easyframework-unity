/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public class AssetImporter : ToolBase<AssetImporter>
    {
        public override int Order => ToolOrder.AssetImporter;

        [MenuItem("EasyFramework/Tools/AssetImporter - Execute", priority = Editor.ToolOrder.AssetImporter)]
        public static void MenuItem()
        {
            Instance.Execute();
            AssetDatabase.Refresh();
        }
    }
}