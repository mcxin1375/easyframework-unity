/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/12/5
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework
{
    public abstract partial class Controller<T>
    {
        public ETask EnterAsync<TK1>(in TK1 tk1)
        {
            if (this is ITParams<TK1> t1) t1.SetParams(in tk1);
            return EnterAsync();
        }
        public ETask EnterAsync<TK1, TK2>(in TK1 tk1, in TK2 tk2)
        {
            if (this is ITParams<TK1, TK2> t2) t2.SetParams(in tk1, in tk2);
            return EnterAsync();
        }
        public ETask EnterAsync<TK1, TK2, TK3>(in TK1 tk1, in TK2 tk2, in TK3 tk3)
        {
            if (this is  ITParams<TK1, TK2, TK3> t3) t3.SetParams(in tk1, in tk2, in tk3);
            return EnterAsync();
        }
    }
}
