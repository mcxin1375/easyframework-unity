/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2026/10/31
// describe: AssetBundle build result inspection window.
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class AssetBundleWindow : EditorWindow
    {
        private const float MinWindowWidth = 760f;
        private const float MinWindowHeight = 420f;
        private const float BundleListWidth = 260f;
        private const float RowHeight = 24f;

        private readonly List<BundleViewData> _bundles = new List<BundleViewData>();
        private readonly Dictionary<string, List<string>> _resourceBundles =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        private AssetBundleBuild[] _configuredBuilds = Array.Empty<AssetBundleBuild>();
        private AssetBundleBuildLayoutReport _lastBuildReport;
        private AssetBundleBuildLayoutReport _lastAnalysisReport;
        private Vector2 _bundleScrollPosition;
        private Vector2 _resourceScrollPosition;
        private string _searchText = string.Empty;
        private string _selectedBundleName;
        private string _refreshError;
        private string _viewMessage;
        private int _viewIndex;
        private bool _showDuplicatesOnly;
        private bool _hasLoaded;
        private bool _isAnalyzing;

        private int DuplicateResourceCount => _resourceBundles.Count(pair => pair.Value.Count > 1);
        private int DuplicateReferenceCount => _resourceBundles
            .Where(pair => pair.Value.Count > 1)
            .Sum(pair => pair.Value.Count);
        private int TotalResourceCount => _bundles.Sum(bundle => bundle.Resources.Length);

        public static void Open()
        {
            var window = GetWindow<AssetBundleWindow>("AssetBundle Window");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Show();
        }

        private void OnEnable()
        {
            minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            RefreshData();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (!_hasLoaded)
            {
                EditorGUILayout.HelpBox("正在读取 AssetBundleBuilder 数据...", MessageType.Info);
                return;
            }

            if (!string.IsNullOrEmpty(_refreshError))
            {
                EditorGUILayout.HelpBox(_refreshError, MessageType.Error);
                return;
            }

            DrawViewHeader();
            DrawSummary();

            if (DuplicateResourceCount > 0)
            {
                EditorGUILayout.HelpBox(
                    $"发现 {DuplicateResourceCount} 个资源路径在多个 AB 包中包含相同对象，共 {DuplicateReferenceCount} 个包内重复项。黄色行表示重复资源。",
                    MessageType.Warning);
            }

            DrawContent(GetVisibleBundles());
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(48f)))
                {
                    RefreshData();
                    GUI.FocusControl(null);
                }

                if (GUILayout.Button("定位配置", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                    SettingsService.OpenProjectSettings("Project/EasyFramework Tools/AssetBundleBuilder");

                var selectedView = GUILayout.Toolbar(_viewIndex,
                    new[] { "当前配置", "最近构建布局", "模拟分析" }, EditorStyles.toolbarButton);
                if (selectedView != _viewIndex)
                {
                    _viewIndex = selectedView;
                    SetActiveViewData();
                    ResetScrollPositions();
                }

                GUILayout.Space(6f);
                var duplicateFilter = GUILayout.Toggle(_showDuplicatesOnly, "仅看重复资源",
                    EditorStyles.toolbarButton, GUILayout.Width(92f));
                if (duplicateFilter != _showDuplicatesOnly)
                {
                    _showDuplicatesOnly = duplicateFilter;
                    ResetScrollPositions();
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label("搜索", EditorStyles.miniLabel, GUILayout.Width(32f));
                var searchFieldStyle = GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.toolbarTextField;
                var searchText = EditorGUILayout.TextField(_searchText, searchFieldStyle,
                    GUILayout.MinWidth(160f), GUILayout.MaxWidth(360f));
                if (!string.Equals(searchText, _searchText, StringComparison.Ordinal))
                {
                    _searchText = searchText;
                    ResetScrollPositions();
                }

                if (!string.IsNullOrEmpty(_searchText) &&
                    GUILayout.Button(GUIContent.none,
                        GUI.skin.FindStyle("ToolbarSearchCancelButton") ?? EditorStyles.toolbarButton,
                        GUILayout.Width(18f)))
                {
                    _searchText = string.Empty;
                    GUI.FocusControl(null);
                }
            }
        }

        private void DrawViewHeader()
        {
            if (_viewIndex == 0)
            {
                EditorGUILayout.LabelField("来自当前 AssetBundleBuilder 配置；列表只显示直接登记到各 AB 的文件。",
                    EditorStyles.miniLabel);
                return;
            }

            var report = _viewIndex == 1 ? _lastBuildReport : _lastAnalysisReport;
            if (report == null)
            {
                if (_viewIndex == 2)
                    DrawAnalysisControls();
                return;
            }

            var reportKind = report.isSimulation ? "模拟分析" : "正式构建";
            EditorGUILayout.LabelField(
                $"{reportKind} · {report.buildTarget} · {report.generatedAt}", EditorStyles.miniLabel);
            if (_viewIndex == 2) DrawAnalysisControls();
        }

        private void DrawAnalysisControls()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    "分析当前配置的 SBP 打包布局；流程在写入 AssetBundle 文件之前结束。",
                    EditorStyles.miniLabel);
                using (new EditorGUI.DisabledScope(_isAnalyzing))
                {
                    if (GUILayout.Button(_isAnalyzing ? "分析中..." : "分析当前配置",
                            EditorStyles.miniButton, GUILayout.Width(112f)))
                        AnalyzeCurrentSettings();
                }
            }
        }

        private void DrawSummary()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                DrawSummaryItem("AB 包", _bundles.Count.ToString());
                DrawSummaryItem("包内资源", TotalResourceCount.ToString());
                DrawSummaryItem("重复资源", DuplicateResourceCount.ToString(), DuplicateResourceCount > 0);
                DrawSummaryItem("重复包内项", DuplicateReferenceCount.ToString(), DuplicateReferenceCount > 0);

                GUILayout.FlexibleSpace();
                var filteredCount = GetVisibleBundles().Count;
                GUILayout.Label($"显示 {filteredCount}/{_bundles.Count} 个 AB 包", EditorStyles.miniLabel,
                    GUILayout.ExpandWidth(false));
            }
        }

        private static void DrawSummaryItem(string title, string value, bool warning = false)
        {
            var previousColor = GUI.color;
            if (warning) GUI.color = new Color(0.85f, 0.45f, 0.08f);

            using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(94f)))
            {
                GUILayout.Label(title, EditorStyles.miniLabel);
                GUILayout.Label(value, EditorStyles.boldLabel);
            }

            GUI.color = previousColor;
        }

        private void DrawContent(List<BundleViewData> visibleBundles)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawBundleList(visibleBundles);
                DrawResourceList(visibleBundles);
            }
        }

        private void DrawBundleList(List<BundleViewData> visibleBundles)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(BundleListWidth)))
            {
                EditorGUILayout.LabelField("AssetBundle", EditorStyles.boldLabel);
                _bundleScrollPosition = EditorGUILayout.BeginScrollView(_bundleScrollPosition);
                if (visibleBundles.Count == 0)
                {
                    EditorGUILayout.HelpBox(
                        _viewMessage ?? (string.IsNullOrWhiteSpace(_searchText)
                            ? "没有可显示的 AB 包。"
                            : "没有匹配的 AB 包。"), MessageType.Info);
                }
                else
                {
                    foreach (var bundle in visibleBundles)
                        DrawBundleRow(bundle);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawBundleRow(BundleViewData bundle)
        {
            var selected = string.Equals(bundle.Name, _selectedBundleName, StringComparison.OrdinalIgnoreCase);
            var duplicateCount = bundle.DuplicateResourceCount;
            var duplicate = duplicateCount > 0;
            var previousBackground = GUI.backgroundColor;

            if (selected)
                GUI.backgroundColor = new Color(0.32f, 0.56f, 0.88f);
            else if (duplicate)
                GUI.backgroundColor = new Color(1f, 0.82f, 0.45f);

            var label = duplicate
                ? $"!  {bundle.Name}   ({bundle.Resources.Length}, 重复 {duplicateCount})"
                : $"    {bundle.Name}   ({bundle.Resources.Length})";
            if (GUILayout.Button(new GUIContent(label, duplicate ? "该 AB 包包含重复对象" : null),
                    EditorStyles.miniButton, GUILayout.Height(RowHeight)))
            {
                _selectedBundleName = bundle.Name;
                _resourceScrollPosition = Vector2.zero;
                Repaint();
            }

            GUI.backgroundColor = previousBackground;
        }

        private void DrawResourceList(List<BundleViewData> visibleBundles)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                var selectedBundle = _bundles.FirstOrDefault(bundle =>
                    string.Equals(bundle.Name, _selectedBundleName, StringComparison.OrdinalIgnoreCase));
                if (selectedBundle == null)
                {
                    EditorGUILayout.HelpBox(
                        visibleBundles.Count == 0 ? "调整搜索条件后选择一个 AB 包。" : "选择一个 AB 包查看资源。",
                        MessageType.Info);
                    return;
                }

                var resources = GetVisibleResources(selectedBundle);
                EditorGUILayout.LabelField(selectedBundle.Name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"资源列表 ({resources.Count}/{selectedBundle.Resources.Length})",
                    EditorStyles.miniLabel);
                DrawResourceHeader();

                _resourceScrollPosition = EditorGUILayout.BeginScrollView(_resourceScrollPosition);
                if (resources.Count == 0)
                {
                    EditorGUILayout.HelpBox("没有匹配的资源。", MessageType.Info);
                }
                else
                {
                    foreach (var resource in resources)
                        DrawResourceRow(resource);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private static void DrawResourceHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("资源路径", EditorStyles.miniLabel);
                GUILayout.Label("来源", EditorStyles.miniLabel, GUILayout.Width(72f));
                GUILayout.Label("状态", EditorStyles.miniLabel, GUILayout.Width(92f));
                GUILayout.Label("所在 AB", EditorStyles.miniLabel, GUILayout.Width(140f));
                GUILayout.Label(string.Empty, GUILayout.Width(44f));
            }
        }

        private void DrawResourceRow(ResourceViewData resource)
        {
            var duplicateBundles = GetDuplicateBundles(resource.AssetPath);
            var duplicate = duplicateBundles != null;
            var previousBackground = GUI.backgroundColor;
            if (duplicate) GUI.backgroundColor = new Color(1f, 0.82f, 0.45f);

            var referencingFiles = resource.ReferencingFiles.Length == 0
                ? string.Empty
                : $"\n由以下直接资源引用:\n{string.Join("\n", resource.ReferencingFiles)}";
            var tooltip = resource.AssetPath + referencingFiles;
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.Height(RowHeight)))
            {
                GUILayout.Label(new GUIContent(resource.AssetPath, tooltip), GUILayout.ExpandWidth(true));
                GUILayout.Label(resource.IsDirect ? "直接资源" : "间接依赖", EditorStyles.miniLabel,
                    GUILayout.Width(72f));
                if (duplicate)
                {
                    var duplicateLabel = resource.HasObjectDuplicateCount
                        ? $"重复对象 x{resource.DuplicateObjectCount}"
                        : $"重复 x{duplicateBundles.Count}";
                    GUILayout.Label(new GUIContent(duplicateLabel, duplicateLabel), EditorStyles.boldLabel,
                        GUILayout.Width(92f));
                    var bundleText = string.Join(", ", duplicateBundles);
                    GUILayout.Label(new GUIContent(bundleText, bundleText), EditorStyles.miniLabel,
                        GUILayout.Width(140f));
                }
                else
                {
                    GUILayout.Label("正常", EditorStyles.miniLabel, GUILayout.Width(92f));
                    GUILayout.Label("-", EditorStyles.miniLabel, GUILayout.Width(140f));
                }

                if (GUILayout.Button("定位", EditorStyles.miniButton, GUILayout.Width(42f)))
                {
                    var asset = AssetDatabase.LoadMainAssetAtPath(resource.AssetPath);
                    if (asset != null)
                    {
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }
                    else
                    {
                        Debug.LogWarning($"AssetBundleWindow: 无法定位资源 {resource.AssetPath}");
                    }
                }
            }

            GUI.backgroundColor = previousBackground;
        }

        private List<BundleViewData> GetVisibleBundles()
        {
            return _bundles.Where(bundle =>
                (!_showDuplicatesOnly || bundle.Resources.Any(resource => IsDuplicate(resource.AssetPath))) &&
                (string.IsNullOrWhiteSpace(_searchText) || BundleMatchesSearch(bundle)))
                .ToList();
        }

        private List<ResourceViewData> GetVisibleResources(BundleViewData bundle)
        {
            var searchMatchesBundle = string.IsNullOrWhiteSpace(_searchText) ||
                ContainsIgnoreCase(bundle.Name, _searchText);
            return bundle.Resources.Where(resource =>
                    (!_showDuplicatesOnly || IsDuplicate(resource.AssetPath)) &&
                    (searchMatchesBundle || ContainsIgnoreCase(resource.AssetPath, _searchText)))
                .ToList();
        }

        private bool BundleMatchesSearch(BundleViewData bundle)
        {
            if (ContainsIgnoreCase(bundle.Name, _searchText)) return true;
            return bundle.Resources.Any(resource => ContainsIgnoreCase(resource.AssetPath, _searchText));
        }

        private static bool ContainsIgnoreCase(string value, string search)
        {
            return value?.IndexOf(search ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void EnsureSelectedBundle(List<BundleViewData> visibleBundles)
        {
            if (visibleBundles.Any(bundle =>
                    string.Equals(bundle.Name, _selectedBundleName, StringComparison.OrdinalIgnoreCase))) return;

            _selectedBundleName = visibleBundles.Count > 0 ? visibleBundles[0].Name : null;
            _resourceScrollPosition = Vector2.zero;
        }

        private bool IsDuplicate(string assetPath)
        {
            return _resourceBundles.TryGetValue(assetPath, out var bundles) && bundles.Count > 1;
        }

        private List<string> GetDuplicateBundles(string assetPath)
        {
            return _resourceBundles.TryGetValue(assetPath, out var bundles) && bundles.Count > 1
                ? bundles
                : null;
        }

        private void RefreshData()
        {
            _hasLoaded = false;
            _refreshError = null;
            try
            {
                ToolExtension<IAssetBundleBuilderSettings>.Refresh();
                _configuredBuilds = AssetBundleBuilderUtility.CreateAssetBundleBuildBySettings() ??
                    Array.Empty<AssetBundleBuild>();
                _lastBuildReport = AssetBundleBuilderUtility.LoadLastBuildLayoutReport();
                _lastAnalysisReport = AssetBundleBuilderUtility.LoadLastAnalysisReport();
                SetActiveViewData();
            }
            catch (Exception exception)
            {
                _refreshError = $"读取 AssetBundleBuilder 数据失败：{exception.Message}";
                Debug.LogException(exception);
            }
            finally
            {
                _hasLoaded = true;
                Repaint();
            }
        }

        private void SetActiveViewData()
        {
            _bundles.Clear();
            _resourceBundles.Clear();
            _viewMessage = null;

            if (_viewIndex == 0)
                LoadConfiguredBuilds();
            else
                LoadBuildLayout(_viewIndex == 1 ? _lastBuildReport : _lastAnalysisReport);

            foreach (var bundle in _bundles)
                bundle.DuplicateResourceCount = bundle.Resources.Count(resource => IsDuplicate(resource.AssetPath));

            EnsureSelectedBundle(GetVisibleBundles());
            Repaint();
        }

        private void LoadConfiguredBuilds()
        {
            foreach (var build in _configuredBuilds.OrderBy(build => build.assetBundleName,
                         StringComparer.OrdinalIgnoreCase))
            {
                var resources = (build.assetNames ?? Array.Empty<string>())
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(path => NormalizeAssetPath(path))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(path => new ResourceViewData(path, true, false, false, 0, Array.Empty<string>()))
                    .ToArray();
                var bundle = new BundleViewData(build.assetBundleName ?? "<未命名 AB>", resources);
                _bundles.Add(bundle);

                foreach (var resource in resources)
                    AddBundleReference(resource.AssetPath, bundle.Name);
            }

            foreach (var bundle in _bundles)
            {
                foreach (var resource in bundle.Resources)
                {
                    var duplicateBundles = GetDuplicateBundles(resource.AssetPath);
                    if (duplicateBundles != null)
                        resource.DuplicateObjectCount = duplicateBundles.Count;
                }
            }
        }

        private void LoadBuildLayout(AssetBundleBuildLayoutReport report)
        {
            if (report?.bundles == null)
            {
                _viewMessage = _viewIndex == 1
                    ? "还没有正式构建布局报告。执行 AssetBundleBuilder 后可在此查看。"
                    : "还没有模拟分析报告。点击“分析当前配置”生成。";
                return;
            }

            foreach (var bundleLayout in report.bundles)
            {
                var resourceLayouts = bundleLayout.resources ?? Array.Empty<AssetBundleBuildLayoutResource>();
                var resources = resourceLayouts
                    .Where(resource => !string.IsNullOrWhiteSpace(resource.assetPath))
                    .Select(resource => new ResourceViewData(
                        NormalizeAssetPath(resource.assetPath), resource.isDirect, true, resource.isDuplicate,
                        resource.duplicateObjectCount, resource.referencingFiles ?? Array.Empty<string>()))
                    .ToArray();
                var bundle = new BundleViewData(bundleLayout.name ?? "<未命名 AB>", resources);
                _bundles.Add(bundle);

                foreach (var resource in resources)
                    if (resource.IsDuplicateInLayout)
                        AddBundleReference(resource.AssetPath, bundle.Name);
            }
        }

        private void AddBundleReference(string assetPath, string bundleName)
        {
            if (!_resourceBundles.TryGetValue(assetPath, out var bundles))
            {
                bundles = new List<string>();
                _resourceBundles.Add(assetPath, bundles);
            }

            if (!bundles.Contains(bundleName, StringComparer.OrdinalIgnoreCase))
                bundles.Add(bundleName);
        }

        private void AnalyzeCurrentSettings()
        {
            _isAnalyzing = true;
            _refreshError = null;
            EditorUtility.DisplayProgressBar("AssetBundle Window", "分析当前打包配置...", 0.5f);
            try
            {
                _lastAnalysisReport = AssetBundleBuilderUtility.AnalyzeBySettings();
                _configuredBuilds = CreateBuildDefinitions(_lastAnalysisReport);
                SetActiveViewData();
            }
            catch (Exception exception)
            {
                _refreshError = $"模拟分析失败：{exception.Message}";
                Debug.LogException(exception);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _isAnalyzing = false;
                _hasLoaded = true;
                Repaint();
            }
        }

        private static AssetBundleBuild[] CreateBuildDefinitions(AssetBundleBuildLayoutReport report)
        {
            if (report?.bundles == null) return Array.Empty<AssetBundleBuild>();

            return report.bundles.Select(bundle => new AssetBundleBuild
                {
                    assetBundleName = bundle.name,
                    assetNames = (bundle.resources ?? Array.Empty<AssetBundleBuildLayoutResource>())
                        .Where(resource => resource.isDirect && !string.IsNullOrWhiteSpace(resource.assetPath))
                        .Select(resource => resource.assetPath)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray()
                })
                .ToArray();
        }

        private void ResetScrollPositions()
        {
            _bundleScrollPosition = Vector2.zero;
            _resourceScrollPosition = Vector2.zero;
        }

        private static string NormalizeAssetPath(string path)
        {
            return path?.Replace('\\', '/');
        }

        private sealed class BundleViewData
        {
            public readonly string Name;
            public readonly ResourceViewData[] Resources;
            public int DuplicateResourceCount;

            public BundleViewData(string name, ResourceViewData[] resources)
            {
                Name = name;
                Resources = resources ?? Array.Empty<ResourceViewData>();
            }
        }

        private sealed class ResourceViewData
        {
            public readonly string AssetPath;
            public readonly bool IsDirect;
            public readonly bool HasObjectDuplicateCount;
            public readonly bool IsDuplicateInLayout;
            public readonly string[] ReferencingFiles;
            public int DuplicateObjectCount;

            public ResourceViewData(string assetPath, bool isDirect, bool hasObjectDuplicateCount,
                bool isDuplicateInLayout,
                int duplicateObjectCount, string[] referencingFiles)
            {
                AssetPath = assetPath;
                IsDirect = isDirect;
                HasObjectDuplicateCount = hasObjectDuplicateCount;
                IsDuplicateInLayout = isDuplicateInLayout;
                DuplicateObjectCount = duplicateObjectCount;
                ReferencingFiles = referencingFiles ?? Array.Empty<string>();
            }
        }
    }
}
