/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

#if EF_HYBRIDCLR

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class HybridCLRBuilderProvider : ProjectSettingsProvider<HybridCLRBuilderProvider>
    {
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public HybridCLRBuilderProvider() : base(EasyFrameworkEditorProvider.ToChildProvider(nameof(HybridCLRBuilder))) { }

        protected override ScriptableObject[] LoadObjects()
        {
            return new ScriptableObject[]
            {
                HybridCLRBuilderSettings.CreateInstance(),
            };
        }
        
        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);
            
            EditorGUILayout.HelpBox($"IEasyFrameworkTool<{nameof(HybridCLRBuilder)}>", MessageType.Info);
            
            foreach (var ex in HybridCLRBuilder.Instance.Extensions)
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

#endif