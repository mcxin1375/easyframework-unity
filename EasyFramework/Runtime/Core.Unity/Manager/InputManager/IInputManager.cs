/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/3/1
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;

namespace EasyFramework
{
    public interface IInputManager
    {
        event Action<EInputType, int, Vector2> OnInputEvent;
    }
}