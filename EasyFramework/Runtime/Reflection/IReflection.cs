/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public interface IReflection
    {
        Type[] Types { get; }
        Type[] InstanceTypes { get; }
    }
}
