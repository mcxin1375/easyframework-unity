/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public interface IAssetCreatorExtension : IEditorToolExtension
    {
        void OnExecute();
    }
    
    public class AssetCreator : EditorTool<AssetCreator>
    {
        public const string ProviderPath = "Project/EasyFramework Editor/2.AssetCreator";
        
        public IAssetCreatorExtension[] Extensions => EditorToolExtension<IAssetCreatorExtension>.Extensions;
        
        [MenuItem("EasyFramework/Tools/AssetCreator - Execute", priority = EasyFrameworkToolsSettings.AssetCreator)]
        public static void MenuItem()
        {
            AssetCreator.Instance.RefreshExtensions();
            AssetCreator.Instance.Execute();
            AssetDatabase.Refresh();
        }

        public void RefreshExtensions()
        {
            EditorToolExtension<IAssetCreatorExtension>.Refresh();
        }

        public void Execute()
        {
            FDebug.Log("AssetCreator - Execute");

            foreach (var extension in Extensions) extension.OnExecute();
            
            FDebug.Log("AssetCreator - OnExecute Completed!");
            
            AssetDatabase.Refresh();
        }
    }
}