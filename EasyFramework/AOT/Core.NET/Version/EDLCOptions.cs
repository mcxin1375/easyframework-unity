/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    [Flags]
    public enum EDLCOptions
    {
        None = 0,
        
        /// <summary>
        /// 原封不动拷贝，不做任何加工，只生成版本信息
        /// </summary>
        DLC = 1,
        
        DLCZip = 2,
    }
}