/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public static class EasyFrameworkToolsExMenuItem
    {
        private const int Priority = EasyFrameworkEditorMenuItemOrder.ToolsEx;
        
        [MenuItem("EasyFramework/Tools Ex/SVNExtension - Update", priority = Priority + 100)]
        private static void SVNExtension_Update() => SVNExtension.Update();
        [MenuItem("EasyFramework/Tools Ex/SVNExtension - Update(cleanup | revert)", priority = Priority + 100)]
        private static void SVNExtension_Update1() => SVNExtension.Update(true, true);
        [MenuItem("EasyFramework/Tools Ex/SVNExtension - Update(cleanup | revert | deleteUnversionedFiles)", priority = Priority + 100)]
        private static void SVNExtension_Update2() => SVNExtension.Update(true, true, true);
        [MenuItem("EasyFramework/Tools Ex/SVNExtension - CommitBySettings", priority = Priority + 100)]
        private static void SVNExtension_Commit() => SVNExtension.CommitBySettings();
    }
}