/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class PlayerBuilderProvider : ProjectSettingsProvider<PlayerBuilderProvider>
    {
        
        [SettingsProvider]
        public static SettingsProvider Create() => GetOrCreate();
        
        public PlayerBuilderProvider() : base(EasyFrameworkEditorProvider.ToChildProvider(nameof(PlayerBuilder))) { }

        protected override ScriptableObject[] LoadObjects()
        {
            EditorToolExtension<IPlayerBuilderExtension>.Refresh();
            
            return new ScriptableObject[]
            {
                PlayerBuilderSettings.CreateInstance(),
            };
        }

        protected override void OnAfterDrawSettings(string settingsName)
        {
            base.OnAfterDrawSettings(settingsName);
            
            EditorGUILayout.HelpBox($"{nameof(IPlayerBuilderExtension)}", MessageType.Info);
            foreach (var ex in PlayerBuilder.Instance.Extensions)
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