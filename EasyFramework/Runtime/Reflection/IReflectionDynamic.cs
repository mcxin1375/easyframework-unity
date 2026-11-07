/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public interface IReflectionDynamic<T>
    {
        static readonly Type[] Types = ReflectionUtility.FindTypes<T>(ReflectionUtility.TagAssemblies);
        static readonly Type[] InstanceTypes = ReflectionUtility.FindInstanceTypes<T>(ReflectionUtility.TagAssemblies);
    }
}
