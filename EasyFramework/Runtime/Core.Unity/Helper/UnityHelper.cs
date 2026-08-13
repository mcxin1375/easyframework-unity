/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/


using System.Linq;

namespace EasyFramework
{
    public static partial class UnityHelper
    {
        public static bool IsEditorPath(this string str)
        {
            string[] arr = str.Replace('\\', '/').Split("/");
            if (arr?.Length > 0)
            {
                if (arr.Contains("Editor") || arr.Contains("EditorResources")) return true;
            }
            return false;
        }
        public static bool IsMetaPath(this string str)
        {
            return str.ToLower().EndsWith(".meta");
        }
    }
}