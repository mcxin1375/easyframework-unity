/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/23
// describe:
//----------------------------------------------------------------*/

using System.Reflection;
using UnityEngine;

namespace EasyFramework
{
    public static partial class UnityHelper
    {
        public static void AutoSetComponents(object obj, GameObject viewObj, BindingFlags bindingFlags = EasyFrameworkReflection.DefaultBindingFlags)
        {
            UnityComponentHelper.AutoSetComponents(obj, viewObj, bindingFlags);
        }
    }
}