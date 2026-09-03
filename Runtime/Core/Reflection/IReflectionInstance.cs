/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public interface IReflectionInstance<T>
    {
        static readonly T Instance = ReflectionUtility.CreateInstance<T>();
    }
}
