/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework.Editor
{

    public partial class SVCCollector : ToolBase<SVCCollector>
    {
        protected override void OnSelfExecute()
        {
            SaveCurrentSVC();
        }

        // [MenuItem("EasyFramework/Tools/SVCCollector - SaveCurrentSVC", priority = EasyFrameworkToolsSettings.SVCCollector)]
        // private static void MenuItem1()
        // {
        //     Instance.Execute();
        // }
        
    }
}