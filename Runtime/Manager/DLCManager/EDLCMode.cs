/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    [Flags]
    public enum EDLCMode
    {
        None = 0,
        
        /// <summary>
        /// 资源列表模式
        /// </summary>
        List = 1,
        
        // DLCZip = 2,
    }
}