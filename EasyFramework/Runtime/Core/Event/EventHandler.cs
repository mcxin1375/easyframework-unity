// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2026/3/6
// // describe:
// //----------------------------------------------------------------*/
//
// namespace EasyFramework
// {
//     public abstract class EventHandler<T> : IEvent<T> where T : IEvent
//     {
//         protected EventHandler()
//         {
//             Event<T>.Add(this);
//         }
//
//         public void Execute(in T t) => OnExecute(in t);
//         protected abstract void OnExecute(in T t);
//     }
// }