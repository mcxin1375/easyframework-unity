//
// using UnityEngine;
//
// namespace EasyFramework.Editor
// {
//     public abstract class ProjectSettingsEditor<T> : ScriptableObject, IProjectSettings where T : ProjectSettingsEditor<T>
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
//             _instance = UnityEditorHelper.LoadProjectSettings<T>();
//             _instance.OnCreate();
//             return _instance;
//         }
//
//         void IProjectSettings.OnCreate()
//         {
//             OnCreate();
//         }
//
//         protected virtual void OnCreate()
//         {
//             
//         }
//     }
// }