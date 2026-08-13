/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/1/22
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework.Editor
{
    public interface IToolEvent<T> where T : SingletonTool<T>, new()
    {
        int Order => 0;
        
        void OnExecuteBefore() { }
        void OnExecute();
        void OnExecuteAfter() { }
    }
}