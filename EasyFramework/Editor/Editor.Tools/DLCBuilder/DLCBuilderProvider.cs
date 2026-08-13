/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System.Text;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class DLCBuilderProvider : ProjectSettingsProvider<DLCBuilderProvider>
    {

        private DLCBuilderPackage[] _dlcPackages;
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public DLCBuilderProvider() : base(EasyFrameworkEditorProvider.ToChildProvider(nameof(DLCBuilder))) { }

        protected override ScriptableObject[] LoadObjects()
        {
            _dlcPackages = DLCBuilder.Instance.GetDLCBuilderPackages(false);
            return new ScriptableObject[]
            {
                DLCBuilderSettings.CreateInstance(),
            };
        }

        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);

            EditorGUILayout.HelpBox("DLCPackage", MessageType.Info);
            
            if (_dlcPackages?.Length > 0)
            {
                foreach (var packageRequest in _dlcPackages)
                {
                    EditorGUILayout.LabelField($"{GUIStyles.MainPrefix}{packageRequest.PackageName}", GUIStyles.MainStyle);
                    foreach (var s in packageRequest.BuildDirectories)
                    {
                        EditorGUILayout.LabelField($"{GUIStyles.DependencyPrefix}{s}", GUIStyles.DependencyStyle);
                    }
                    EditorGUILayout.Space(5);
                }
            }
            
            EditorGUILayout.HelpBox($"IEasyFrameworkTool<{nameof(DLCBuilder)}>", MessageType.Info);
            
            foreach (var ex in DLCBuilder.Instance.Extensions)
            {
                if (ex is ScriptableObject o)
                {
                    EditorGUILayout.ObjectField($"Order: {ex.Order}", o, o.GetType(), false);
                }
                else
                {
                    EditorGUILayout.LabelField($"Order: {ex.Order}", ex.GetType().Name);
                }
            }
        }
    }
}