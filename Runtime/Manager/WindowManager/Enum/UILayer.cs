/*----------------------------------------------------------------
// author:meng cheng xin
// date:2018/3/1
// describe:UI管理类
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    [Flags]
    public enum UILayer
    {
        HUD = 1,
        Popup = 2,
        Notice = 4,
        Higher = 8,
        Loading = 16,
        All = HUD | Popup | Notice | Higher | Loading
    }
}