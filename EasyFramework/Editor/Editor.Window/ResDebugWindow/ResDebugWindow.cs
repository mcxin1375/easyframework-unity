using System.Text;
using UnityEditor;
using UnityEngine.Build.Pipeline;
using UnityEngine.UIElements;

namespace EasyFramework.Editor
{
    public class ResDebugWindow : UIToolkitEditorWindow<ResDebugWindow>
    {
        public TextElement LabText;
        public TextField TextFieldSearch;
        // public RadioButtonGroup UI_RadioGroup;
        
        public ResDebugListViewEx ListViewEx = new ();
        public ResDebugTreeViewEx TreeViewEx = new ();

        private AssetBundleBuild[] _assetBundleBuilds;
        private AssetBundleBuildDepsDebug _assetBundleBuildDepsDebug;
        private AssetBundleBuildFileTreeDebug _assetBundleBuildFileTreeDebug;
        private int _selectType;
        private string _searchStr;
        private string _selectTab;
        
        protected override void OnOpen()
        {
            base.OnOpen();

            // UI_RadioGroup.style.display = 
            // var rb1 = new RadioButton("DLCList");
            // var rb2 = new RadioButton("DLCZip");
            // UI_RadioGroup.Add(rb1);
            // UI_RadioGroup.Add(rb2);
            // UI_RadioGroup.value = 0;
            // UI_RadioGroup.RegisterValueChangedCallback((evt) =>
            // {
            //     _selectType = evt.newValue;
            //     SelectTab(_selectTab);
            // });

            _assetBundleBuilds = AssetBundleBuilderHelper.CreateAssetBundleBuildBySettings();
            
            ListViewEx.Refresh(new[]
            {
                "DataBuild",
                "AssetBundleBuild",
                "AssetBundleBuildDeps",
                "AssetBundleBuildManifest",
                "AssetBundleBuildFileTree", 
                "ShaderVariantCollection",
                "DLCPackageResList",
                "DLCPackageResZip",
            });
            SelectTab(_selectTab);
        }

        private void OnInspectorUpdate()
        {
            if (_selectTab == null) return;
            
            if (_searchStr != TextFieldSearch.value)
            {
                _searchStr = TextFieldSearch.value;
                SelectTab(_selectTab);
            }
        }

        protected override void OnButtonClick(Button btn)
        {
            switch (btn.name)
            {
                case "Refresh":
                    SelectTab(_selectTab, true);
                    break;
            }
        }

        public void SelectTab(string tab, bool refresh = false)
        {
            _selectTab = tab;
            if (string.IsNullOrWhiteSpace(_selectTab)) return;
            
            LabText.text = string.Empty;

            switch (tab)
            {
                case "DataBuild":
                    SelectDataBuild(refresh);
                    break;
                case "AssetBundleBuild":
                    SelectAssetBundleBuild(refresh);
                    break;
                case "AssetBundleBuildDeps":
                    SelectAssetBundleBuildDeps(refresh);
                    break;
                case "AssetBundleBuildManifest":
                    SelectAssetBundleBuildManifest(refresh);
                    break;
                case "AssetBundleBuildFileTree":
                    SelectAssetBundleBuildFileTree(refresh);
                    break;
                case "ShaderVariantCollection":
                    SelectShaderVariantCollection(refresh);
                    break;
                case "DLCPackageResList":
                    SelectDLCPackageResList(refresh);
                    break;
                case "DLCPackageResZip":
                    SelectDLCPackageResZip(refresh);
                    break;
            }
        }

        private void SelectDataBuild(bool refresh = false)
        {
            var dataFiles = DataBuilder.Instance.GetDataFiles();
            TreeViewEx.Update(dataFiles);
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"DataBuild Count: {dataFiles?.Length ?? 0}");
            LabText.text = sb.ToString();
        }
        private void SelectAssetBundleBuild(bool refresh = false)
        {
            if (refresh)
            {
                _assetBundleBuilds = AssetBundleBuilderHelper.CreateAssetBundleBuildBySettings();
            }
            
            TreeViewEx.Update(_assetBundleBuilds);
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"AssetBundleBuild Count: {_assetBundleBuilds?.Length ?? 0}");
            LabText.text = sb.ToString();
        }
        private void SelectAssetBundleBuildDeps(bool refresh = false)
        {
            if (_assetBundleBuildDepsDebug == null || refresh)
            {
                _assetBundleBuilds = AssetBundleBuilderHelper.CreateAssetBundleBuildBySettings();
                _assetBundleBuildDepsDebug = new AssetBundleBuildDepsDebug(_assetBundleBuilds);
            }
            TreeViewEx.Update(_assetBundleBuildDepsDebug);
        }
        private void SelectAssetBundleBuildManifest(bool refresh = false)
        {
            // var compatibilityAssetBundleManifest = AssetDatabase.LoadAssetAtPath<CompatibilityAssetBundleManifest>(AssetBundleBuilderHelper.ManifestFile);
            // TreeViewEx.Update(compatibilityAssetBundleManifest);
        }
        private void SelectAssetBundleBuildFileTree(bool refresh = false)
        {
            if (_assetBundleBuildFileTreeDebug == null || refresh)
            {
                _assetBundleBuilds = AssetBundleBuilderHelper.CreateAssetBundleBuildBySettings();
                _assetBundleBuildFileTreeDebug = new AssetBundleBuildFileTreeDebug(_assetBundleBuilds);
            }
            TreeViewEx.UpdateAssetBundleBuildTreeInfo(_assetBundleBuildFileTreeDebug);
        }
        private void SelectShaderVariantCollection(bool refresh = false)
        {
            var shaderVariantCollectionInfo = SVCCollector.Instance.CreateShaderVariantCollectionInfo();
            TreeViewEx.Update(shaderVariantCollectionInfo);
        }
        private void SelectDLCPackageResList(bool refresh = false)
        {
            var requests = DLCBuilder.Instance.GetDLCBuilderPackages();
            TreeViewEx.UpdatePackageList(requests);
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Package Count: {requests.Length}");
            LabText.text = sb.ToString();
        }
        private void SelectDLCPackageResZip(bool refresh = false)
        {
            var requests = DLCBuilder.Instance.GetDLCBuilderPackages();
            TreeViewEx.UpdatePackageZip(requests);
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Package Count: {requests.Length}");
            LabText.text = sb.ToString();
        }


    }
}