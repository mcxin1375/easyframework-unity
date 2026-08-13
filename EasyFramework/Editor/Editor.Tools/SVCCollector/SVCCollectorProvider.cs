/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class SVCCollectorProvider : ProjectSettingsProvider<SVCCollectorProvider>
    {
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public SVCCollectorProvider() : base(EasyFrameworkEditorProvider.ToChildProvider(nameof(SVCCollector))) { }

        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                SVCCollectorSettings.CreateInstance(),
            };
        }

        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);
            
            // EditorGUILayout.HelpBox($"SVC File : {ShaderVariantCollectionSettings.Instance.SaveFile}", MessageType.Info);

            var settings = SVCCollectorSettings.Instance;
            
            var newVal = EditorGUILayout.TextField("SvcFileName", settings.SvcFileName);
            if (newVal != settings.SvcFileName)
            {
                settings.SvcFileName = newVal;
            }
            EditorGUILayout.LabelField("SvcSaveFile", settings.SvcSaveFile);
            
            // GUILayout.BeginHorizontal();
            // GUILayout.FlexibleSpace();
            // if (GUILayout.Button("SVN Commit",  GUILayout.Height(30)))
            // {
            //     FEditor.SVCCollector.SaveSVCFile();
            // }
            // GUILayout.EndHorizontal();
            
            
            EditorGUILayout.HelpBox($"IEasyFrameworkTool<{nameof(SVCCollector)}>", MessageType.Info);
            
            foreach (var ex in SVCCollector.Instance.Extensions)
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