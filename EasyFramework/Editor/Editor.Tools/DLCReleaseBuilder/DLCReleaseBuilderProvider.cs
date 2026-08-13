/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class DLCReleaseBuilderProvider : ProjectSettingsProvider<DLCReleaseBuilderProvider>
    {

        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public DLCReleaseBuilderProvider() : base(EasyFrameworkEditorProvider.ToChildProvider(nameof(DLCReleaseBuilder))) { }

        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                // DLCBuilderSettings.CreateInstance(),
            };
        }

        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);

            // EditorGUILayout.HelpBox("DLCPackage", MessageType.Info);
            //
            // if (_dlcPackages?.Length > 0)
            // {
            //     foreach (var packageRequest in _dlcPackages)
            //     {
            //         EditorGUILayout.LabelField($"{GUIStyles.MainPrefix}{packageRequest.PackageName}", GUIStyles.MainStyle);
            //         foreach (var s in packageRequest.BuildDirectories)
            //         {
            //             EditorGUILayout.LabelField($"{GUIStyles.DependencyPrefix}{s}", GUIStyles.DependencyStyle);
            //         }
            //         EditorGUILayout.Space(5);
            //     }
            // }
            //
            // EditorGUILayout.HelpBox($"IEasyFrameworkTool<{nameof(DLCBuilder)}>", MessageType.Info);
            //
            // foreach (var ex in DLCBuilder.Instance.Extensions)
            // {
            //     if (ex is ScriptableObject o)
            //     {
            //         EditorGUILayout.ObjectField($"Order: {ex.Order}", o, o.GetType(), false);
            //     }
            //     else
            //     {
            //         EditorGUILayout.LabelField($"Order: {ex.Order}", ex.GetType().Name);
            //     }
            // }
        }
    }
}