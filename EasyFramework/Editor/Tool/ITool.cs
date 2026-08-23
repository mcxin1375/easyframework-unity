/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2025/1/22
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework.Editor
{
    public interface ITool
    {
        int Order => 0;
        IToolExtension[] Extension => null;
        
        void Refresh();
        void Execute();
    }
    
    public interface IToolExtension
    {
        int Order => 0;
    }
    
    public interface IToolEvent<T> : IToolExtension where T : ToolBase<T>, new()
    {
        void OnExecuteBefore() { }
        void OnExecute() { }
        void OnExecuteAfter() { }
    }
}