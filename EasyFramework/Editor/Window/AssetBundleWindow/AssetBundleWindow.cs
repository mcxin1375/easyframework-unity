/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2026/10/31
// describe: AssetBundle build result inspection window.
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly Dictionary<string, List<string>> _resourceLocations =
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
                var duplicateMessage = _viewIndex == 0
                    ? $"发现 {DuplicateResourceCount} 个资源路径被多个 AB 配置引用，共 {DuplicateReferenceCount} 个包内引用项。黄色行表示重复引用。"
                    : $"发现 {DuplicateResourceCount} 个资源路径在多个 AB 包中包含相同对象，共 {DuplicateReferenceCount} 个包内重复项。黄色行表示重复资源。";
                EditorGUILayout.HelpBox(
                    duplicateMessage,
                    MessageType.Warning);
            }

            DrawContent(GetVisibleBundles());
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var selectedView = GUILayout.Toolbar(_viewIndex,
                    new[] { "构建列表", "最近构建", "模拟分析" }, EditorStyles.toolbarButton);
                if (selectedView != _viewIndex)
                {
                    _viewIndex = selectedView;
                    SetActiveViewData();
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

                GUILayout.Space(6f);
                var duplicateFilter = EditorGUILayout.ToggleLeft("仅看重复资源", _showDuplicatesOnly,
                    GUILayout.Width(112f));
                if (duplicateFilter != _showDuplicatesOnly)
                {
                    _showDuplicatesOnly = duplicateFilter;
                    ResetScrollPositions();
                }

                GUILayout.Space(6f);
                if (GUILayout.Button("配置", EditorStyles.toolbarButton, GUILayout.Width(48f)))
                    SettingsService.OpenProjectSettings("Project/EasyFramework Tools/AssetBundleBuilder");

                if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(48f)))
                {
                    RefreshData();
                    GUI.FocusControl(null);
                }
            }
        }

        private void DrawViewHeader()
        {
            if (_viewIndex == 0)
            {
                EditorGUILayout.LabelField("来自当前 AssetBundleBuilder 配置；列表包含直接登记资源及其递归引用的非代码依赖。",
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

            var label = FormatBundleLabel(bundle);
            var tooltip = duplicate ? $"该 AB 包包含 {duplicateCount} 个重复资源路径。" : null;
            if (GUILayout.Button(new GUIContent(label, tooltip),
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

                EditorGUILayout.LabelField(selectedBundle.Name, EditorStyles.boldLabel);
                GUILayout.Space(6f);

                _resourceScrollPosition = EditorGUILayout.BeginScrollView(_resourceScrollPosition);
                var resources = GetVisibleResources(selectedBundle);
                var directResources = resources.Where(resource => resource.IsDirect).ToList();
                var indirectResources = resources.Where(resource => !resource.IsDirect).ToList();
                var directCount = selectedBundle.Resources.Count(resource => resource.IsDirect);
                var indirectCount = selectedBundle.Resources.Length - directCount;
                DrawResourceSection("直接资源", directResources, directCount);
                GUILayout.Space(8f);
                DrawResourceSection("间接资源", indirectResources, indirectCount);

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawResourceSection(string title, List<ResourceViewData> resources, int totalCount)
        {
            EditorGUILayout.LabelField($"{title} ({resources.Count}/{totalCount})", EditorStyles.boldLabel);
            DrawResourceHeader();
            if (resources.Count == 0)
            {
                EditorGUILayout.HelpBox($"没有可显示的{title}。", MessageType.Info);
                return;
            }

            foreach (var resource in resources)
                DrawResourceRow(resource);
        }

        private static void DrawResourceHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("资源路径", EditorStyles.miniLabel);
                GUILayout.Label("引用数", EditorStyles.miniLabel, GUILayout.Width(58f));
                GUILayout.Label(string.Empty, GUILayout.Width(44f));
            }
        }

        private void DrawResourceRow(ResourceViewData resource)
        {
            var duplicate = GetDuplicateBundles(resource.AssetPath) != null;
            var previousBackground = GUI.backgroundColor;
            if (duplicate) GUI.backgroundColor = new Color(1f, 0.82f, 0.45f);

            var resourceBundles = GetResourceLocations(resource.AssetPath);
            var bundleInfo = resourceBundles == null || resourceBundles.Count == 0
                ? "\n所在 AB：未记录"
                : $"\n所在 AB：\n{string.Join("\n", resourceBundles)}";
            var referencingFiles = resource.ReferencingFiles.Length == 0
                ? string.Empty
                : $"\n由以下直接资源引用:\n{string.Join("\n", resource.ReferencingFiles)}";
            var tooltip = resource.AssetPath + bundleInfo + referencingFiles;
            var resourceTooltip = new GUIContent(resource.AssetPath, tooltip);
            var referenceCountTooltip = new GUIContent((resourceBundles?.Count ?? 0).ToString(), tooltip);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.Height(RowHeight)))
            {
                GUILayout.Label(resourceTooltip, GUILayout.ExpandWidth(true));
                GUILayout.Label(referenceCountTooltip, EditorStyles.miniLabel,
                    GUILayout.Width(58f));

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
            _resourceLocations.Clear();
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
                var directPaths = (build.assetNames ?? Array.Empty<string>())
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(path => NormalizeAssetPath(path))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                var resources = CreateConfiguredResources(directPaths);
                var bundle = new BundleViewData(build.assetBundleName ?? "<未命名 AB>", resources);
                _bundles.Add(bundle);

                foreach (var resource in resources)
                {
                    AddResourceLocation(resource.AssetPath, bundle.Name);
                    AddBundleReference(resource.AssetPath, bundle.Name);
                }
            }
        }

        private static ResourceViewData[] CreateConfiguredResources(string[] directPaths)
        {
            var resources = new List<ResourceViewData>(directPaths.Length);
            var directPathSet = new HashSet<string>(directPaths, StringComparer.OrdinalIgnoreCase);
            var dependencyPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var referencingFiles = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var directPath in directPaths)
            {
                var dependencies = AssetDatabase.GetDependencies(directPath, true);
                foreach (var dependency in dependencies ?? Array.Empty<string>())
                {
                    var dependencyPath = NormalizeAssetPath(dependency);
                    if (string.IsNullOrWhiteSpace(dependencyPath) ||
                        string.Equals(dependencyPath, directPath, StringComparison.OrdinalIgnoreCase) ||
                        IsCodeAsset(dependencyPath)) continue;

                    AddReferencingFile(referencingFiles, dependencyPath, directPath);
                    if (!directPathSet.Contains(dependencyPath)) dependencyPaths.Add(dependencyPath);
                }
            }

            resources.AddRange(directPaths.Select(path => new ResourceViewData(
                path, true, false, GetReferencingFiles(referencingFiles, path))));
            resources.AddRange(dependencyPaths
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .Select(path => new ResourceViewData(
                    path, false, false, GetReferencingFiles(referencingFiles, path))));
            return resources.ToArray();
        }

        private static void AddReferencingFile(Dictionary<string, HashSet<string>> referencingFiles,
            string assetPath, string directPath)
        {
            if (!referencingFiles.TryGetValue(assetPath, out var files))
            {
                files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                referencingFiles.Add(assetPath, files);
            }

            files.Add(directPath);
        }

        private static string[] GetReferencingFiles(Dictionary<string, HashSet<string>> referencingFiles,
            string assetPath)
        {
            return referencingFiles.TryGetValue(assetPath, out var files)
                ? files.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray()
                : Array.Empty<string>();
        }

        private static string FormatBundleLabel(BundleViewData bundle)
        {
            var directCount = bundle.Resources.Count(resource => resource.IsDirect);
            var indirectCount = bundle.Resources.Length - directCount;
            var counts = new List<string>(2);
            if (directCount > 1) counts.Add(directCount.ToString());
            if (indirectCount > 0) counts.Add($"!{indirectCount}");

            return counts.Count == 0
                ? bundle.Name
                : $"{bundle.Name} ({string.Join(", ", counts)})";
        }

        private static bool IsCodeAsset(string assetPath)
        {
            if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(MonoScript)) return true;

            switch (Path.GetExtension(assetPath).ToLowerInvariant())
            {
                case ".cs":
                case ".js":
                case ".boo":
                case ".asmdef":
                case ".asmref":
                case ".dll":
                case ".winmd":
                    return true;
                default:
                    return false;
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
                        NormalizeAssetPath(resource.assetPath), resource.isDirect, resource.isDuplicate,
                        resource.referencingFiles ?? Array.Empty<string>()))
                    .ToArray();
                var bundle = new BundleViewData(bundleLayout.name ?? "<未命名 AB>", resources);
                _bundles.Add(bundle);

                foreach (var resource in resources)
                {
                    AddResourceLocation(resource.AssetPath, bundle.Name);
                    if (resource.IsDuplicateInLayout)
                        AddBundleReference(resource.AssetPath, bundle.Name);
                }
            }
        }

        private void AddResourceLocation(string assetPath, string bundleName)
        {
            if (!_resourceLocations.TryGetValue(assetPath, out var bundles))
            {
                bundles = new List<string>();
                _resourceLocations.Add(assetPath, bundles);
            }

            if (!bundles.Contains(bundleName, StringComparer.OrdinalIgnoreCase))
                bundles.Add(bundleName);
        }

        private List<string> GetResourceLocations(string assetPath)
        {
            return _resourceLocations.TryGetValue(assetPath, out var bundles) ? bundles : null;
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
            _refreshError = null;
            var progressShown = false;
            try
            {
                if (!PrepareAnalysisAssets()) return;

                _isAnalyzing = true;
                EditorUtility.DisplayProgressBar("AssetBundle Window", "分析当前打包配置...", 0.5f);
                progressShown = true;
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
                if (progressShown) EditorUtility.ClearProgressBar();
                _isAnalyzing = false;
                _hasLoaded = true;
                Repaint();
            }
        }

        private static bool PrepareAnalysisAssets()
        {
            var dirtyPaths = AssetBundleBuilderUtility.GetDirtyProjectAssetPaths();
            if (dirtyPaths.Length == 0) return true;

            var previewPaths = dirtyPaths.Take(8).ToArray();
            var preview = string.Join("\n", previewPaths);
            if (dirtyPaths.Length > previewPaths.Length) preview += "\n...";

            var message = $"检测到 {dirtyPaths.Length} 个项目资源存在未保存修改：\n\n{preview}\n\n模拟分析会先保存这些修改，是否继续？";
            if (!EditorUtility.DisplayDialog("模拟分析需要保存资源", message, "保存并继续", "取消"))
                return false;

            AssetDatabase.SaveAssets();
            return true;
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
            public readonly bool IsDuplicateInLayout;
            public readonly string[] ReferencingFiles;

            public ResourceViewData(string assetPath, bool isDirect, bool isDuplicateInLayout,
                string[] referencingFiles)
            {
                AssetPath = assetPath;
                IsDirect = isDirect;
                IsDuplicateInLayout = isDuplicateInLayout;
                ReferencingFiles = referencingFiles ?? Array.Empty<string>();
            }
        }
    }
}
