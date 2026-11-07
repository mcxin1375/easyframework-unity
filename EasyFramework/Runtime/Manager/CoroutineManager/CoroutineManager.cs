// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2018/3/1
// // describe:
// //----------------------------------------------------------------*/
//
// using System.Collections;
// using System.Collections.Generic;
//
// namespace EasyFramework
// {
//     internal class CoroutineManager : SingletonBehaviour<CoroutineManager>, ICoroutineManager
//     {
//         private readonly Dictionary<int, CoroutineTask> _coroutines = new();
//         private int _mCoroutineID = 1;
//
//         protected override void OnAwake()
//         {
//             transform.SetParent(F.Behaviour.transform);
//         }
//
//         protected override void OnDestroyEx()
//         {
//             foreach (var task in _coroutines.Values) task.Running = false;
//             _coroutines.Clear();
//         }
//         
//         int ICoroutineManager.StartCoroutine(IEnumerator co) => StartCoroutineEx(co);
//         public int StartCoroutineEx(IEnumerator co)
//         {
//             if (gameObject.activeSelf)
//             {
//                 CoroutineTask task = new CoroutineTask(GetCoroutineID());
//                 _coroutines.Add(task.Id, task);
//                 StartCoroutine(task.CoroutineWrapper(co));
//                 return task.Id;
//             }
//             return -1;
//         }
//
//         void ICoroutineManager.StopCoroutine(int id) => StopCoroutineEx(id);
//         public void StopCoroutineEx(int id)
//         {
//             if (_coroutines.TryGetValue(id, out var task))
//             {
//                 task.Running = false;
//                 _coroutines.Remove(id);
//             }
//         }
//
//         public void PauseCoroutine(int id)
//         {
//             if (_coroutines.TryGetValue(id, out var task))
//             {
//                 task.Paused = true;
//             }
//         }
//
//         public void ResumeCoroutine(int id)
//         {
//             if (_coroutines.TryGetValue(id, out var task))
//             {
//                 task.Paused = false;
//             }
//         }
//
//         private int GetCoroutineID()
//         {
//             return _mCoroutineID++;
//         }
//         
//         private class CoroutineTask
//         {
//             public int Id { get; set; }
//             public bool Running { get; set; }
//             public bool Paused { get; set; }
//
//             public CoroutineTask(int id)
//             {
//                 Id = id;
//                 Running = true;
//                 Paused = false;
//             }
//
//             public IEnumerator CoroutineWrapper(IEnumerator co)
//             {
//                 IEnumerator coroutine = co;
//                 while (Running)
//                 {
//                     if (Paused)
//                         yield return null;
//                     else
//                     {
//                         if (coroutine != null && coroutine.MoveNext())
//                             yield return coroutine.Current;
//                         else
//                             Running = false;
//                     }
//                 }
//
//                 if (HasInstance()) Instance.StopCoroutineEx(Id);
//             }
//         }
//     }
// }
