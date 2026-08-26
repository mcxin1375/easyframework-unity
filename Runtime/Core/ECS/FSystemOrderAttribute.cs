/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe: 
//----------------------------------------------------------------*/

using System;

namespace EasyFramework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FSystemOrderAttribute : Attribute
    {
        public readonly int Order;
        public FSystemOrderAttribute(int order)
        {
            Order = order;
        }
    }
}