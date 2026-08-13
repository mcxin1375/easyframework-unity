/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    public interface IPoolDebug
    {
        int CreatedCount { get; }
        int PooledCount { get; }
        Type ObjectType { get; }
    }
}