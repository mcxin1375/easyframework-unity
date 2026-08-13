/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;

namespace EasyFramework.Editor
{
    public interface IAssetImporterExtension : IEditorToolExtension
    {
        void OnExecute();
    }

    public class AssetImporter : EditorTool<AssetImporter>
    {
        public const string ProviderPath = "Project/EasyFramework Editor/1.AssetImporter";
        
        public IAssetImporterExtension[] Extensions => EditorToolExtension<IAssetImporterExtension>.Extensions;
        
        [MenuItem("EasyFramework/Tools/AssetImporter - Execute", priority = EasyFrameworkToolsSettings.AssetImporter)]
        public static void MenuItem()
        {
            AssetImporter.Instance.RefreshExtensions();
            AssetImporter.Instance.Execute();
            AssetDatabase.Refresh();
        }

        public void RefreshExtensions()
        {
            EditorToolExtension<IAssetImporterExtension>.Refresh();
        }
        
        public void Execute()
        {
            FDebug.Log("AssetImporter - OnExecute");

            foreach (var extension in Extensions) extension.OnExecute();
            
            FDebug.Log("AssetImporter - OnExecute Completed!");
            
            AssetDatabase.Refresh();
        }
    }
}