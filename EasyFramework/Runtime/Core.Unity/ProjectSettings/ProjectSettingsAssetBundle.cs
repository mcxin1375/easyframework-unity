// /*----------------------------------------------------------------
// // author: Cookie(mcx)
// // date: 2023/11/8
// // describe:
// //----------------------------------------------------------------*/
//
// using UnityEngine;
//
// namespace EasyFramework
// {
//     public abstract class ProjectSettings<T> : ScriptableObject where T : ProjectSettings<T>
//     {
//         private static T _instance;
//         public static T Instance
//         {
//             get
//             {
//                 if (_instance == null) CreateInstance();
//                 return _instance;
//             }
//         }
//
//         public static T CreateInstance()
//         {
// #if UNITY_EDITOR
//             if (_instance == null) _instance = ReLoadOnEditorOnly();
// #endif
//             if (_instance == null) _instance = LoadFromResources();
//             return _instance;
//         }
//         
// #if UNITY_EDITOR
//         public static T ReLoadOnEditorOnly()
//         {
//             _instance = EditorBridge.LoadProjectSetting<T>();
//             _instance.OnCreate();
//             return _instance;
//         }
// #endif
//
//         private static T LoadFromResources()
//         {
//             _instance = Resources.Load<T>(typeof(T).Name);
//             if (_instance == null)
//                 _instance = ScriptableObject.CreateInstance<T>();
//             _instance.OnCreate();
//             return _instance;
//         }
//
//         // public static async ETask<T> LoadAsync()
//         // {
//         //     _instance = await F.ResLoader.LoadAssetAsync<T>(typeof(T).Name);
//         //     _instance ??= CreateInstance<T>();
//         //     _instance.OnCreate();
//         //     return _instance;
//         // }
//         
//         protected virtual void OnCreate() { }
//     }
// }