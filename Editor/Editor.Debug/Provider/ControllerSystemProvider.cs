/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class ControllerSystemProvider : ProjectSettingsProvider<ControllerSystemProvider>
    {
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public ControllerSystemProvider() : base(EasyFrameworkProvider.ToChildProvider<ControllerManager>()) { }

        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                // settings,
            };
        }

        protected override void OnAfterDraw()
        {
            base.OnAfterDraw();

            if (!ControllerManager.HasInstance()) return;
            
            EditorGUILayout.HelpBox("EnterList", MessageType.Info);
            foreach (var controller in ControllerManager.Instance.EnterList)
            {
                EditorGUILayout.LabelField($"{controller.GetType().Name}", $"IsEnter: {controller.IsEnter}, IsActive: {controller.IsActive}");
            }
        }
    }
}