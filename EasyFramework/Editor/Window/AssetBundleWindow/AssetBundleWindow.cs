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
using UnityEditor.SceneManagement;
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
        private readonly Dictionary<string, List<string>> _contentResourceBundles =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<string>> _resourceLocations =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ResourceCategory> _resourceCategories =
            new Dictionary<string, ResourceCategory>(StringComparer.OrdinalIgnoreCase);

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
        private bool _showMultipleDirectResourcesOnly;
        private bool _includeCode;
        private bool _includeShaders;
        private bool _hasLoaded;
        private bool _isAnalyzing;

        private int DuplicateResourceCount => _resourceBundles.Count(pair =>
            IsResourcePathVisible(pair.Key) && pair.Value.Count > 1);
        private int DuplicateReferenceCount => _resourceBundles
            .Where(pair => IsResourcePathVisible(pair.Key) && pair.Value.Count > 1)
            .Sum(pair => pair.Value.Count);
        private int ContentDuplicateResourceCount => _contentResourceBundles.Count(pair =>
            IsResourcePathVisible(pair.Key) && pair.Value.Count > 1);
        private long EstimatedDuplicateByteCount => _bundles.Sum(bundle => bundle.Resources
            .Where(IsResourceVisible)
            .Sum(resource => resource.DuplicateObjectBytes + resource.ContentDuplicateBytes));
        private bool ContentAnalysisAvailable => _viewIndex > 0 &&
            (_viewIndex == 1 ? _lastBuildReport : _lastAnalysisReport)?.hasContentAnalysis == true;
        private int TotalResourceCount => _bundles.Sum(bundle =>
            bundle.Resources.Count(IsResourceVisible));

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

            if (DuplicateResourceCount > 0 || ContentAnalysisAvailable && ContentDuplicateResourceCount > 0)
            {
                var duplicateMessage = _viewIndex == 0
                    ? $"发现 {DuplicateResourceCount} 个资源路径被多个 AB 配置引用，共 {DuplicateReferenceCount} 个配置项。"
                    : $"同对象重复 {DuplicateResourceCount} 个资源、{DuplicateReferenceCount} 个包内项；内容重复 {ContentDuplicateResourceCount} 个资源。黄色行表示重复证据。";
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
                var duplicateFilter = EditorGUILayout.ToggleLeft("重复资源", _showDuplicatesOnly,
                    GUILayout.Width(86f));
                if (duplicateFilter != _showDuplicatesOnly)
                {
                    _showDuplicatesOnly = duplicateFilter;
                    OnFilterChanged();
                }

                GUILayout.Space(4f);
                var codeFilter = EditorGUILayout.ToggleLeft("代码", _includeCode, GUILayout.Width(52f));
                if (codeFilter != _includeCode)
                {
                    _includeCode = codeFilter;
                    OnFilterChanged();
                }

                GUILayout.Space(4f);
                var shaderFilter = EditorGUILayout.ToggleLeft("Shader", _includeShaders, GUILayout.Width(64f));
                if (shaderFilter != _includeShaders)
                {
                    _includeShaders = shaderFilter;
                    OnFilterChanged();
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

        private void OnFilterChanged()
        {
            ResetScrollPositions();
            EnsureSelectedBundle(GetVisibleBundles());
            Repaint();
        }

        private bool IsResourceVisible(ResourceViewData resource)
        {
            if (!_includeCode && resource.IsCode) return false;
            if (!_includeShaders && resource.IsShader) return false;
            return true;
        }

        private bool IsResourcePathVisible(string assetPath)
        {
            if (!_resourceCategories.TryGetValue(assetPath, out var category))
                category = GetResourceCategory(assetPath);

            if (!_includeCode && category == ResourceCategory.Code) return false;
            if (!_includeShaders && category == ResourceCategory.Shader) return false;
            return true;
        }

        private void DrawViewHeader()
        {
            if (_viewIndex == 0)
            {
                EditorGUILayout.LabelField("来自当前 AssetBundleBuilder 配置；列表按顶部开关显示直接资源及其递归依赖。",
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
                    "分析当前配置的 SBP 布局；只写入隔离临时序列化文件，不归档正式 AssetBundle。",
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
                DrawSummaryItem(_viewIndex == 0 ? "重复配置资源" : "同对象重复",
                    DuplicateResourceCount.ToString(), DuplicateResourceCount > 0);
                DrawSummaryItem(_viewIndex == 0 ? "重复配置项" : "同对象项",
                    DuplicateReferenceCount.ToString(), DuplicateReferenceCount > 0);
                DrawSummaryItem("内容重复", ContentAnalysisAvailable ? ContentDuplicateResourceCount.ToString() : "—",
                    ContentAnalysisAvailable && ContentDuplicateResourceCount > 0);
                DrawSummaryItem("估算重复字节", ContentAnalysisAvailable ? FormatBytes(EstimatedDuplicateByteCount) : "—",
                    ContentAnalysisAvailable && EstimatedDuplicateByteCount > 0);

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

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024f:0.##} KB";
            if (bytes < 1024L * 1024L * 1024L) return $"{bytes / (1024f * 1024f):0.##} MB";
            return $"{bytes / (1024f * 1024f * 1024f):0.##} GB";
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
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("AssetBundle", EditorStyles.boldLabel);
                    var multipleResourceFilter = EditorGUILayout.ToggleLeft("直接资源数>1",
                        _showMultipleDirectResourcesOnly, GUILayout.Width(94f));
                    if (multipleResourceFilter != _showMultipleDirectResourcesOnly)
                    {
                        _showMultipleDirectResourcesOnly = multipleResourceFilter;
                        OnFilterChanged();
                    }
                }
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
            var duplicateCount = bundle.Resources.Count(resource =>
                IsResourceVisible(resource) && IsAnyDuplicate(resource.AssetPath));
            var exactDuplicateCount = bundle.Resources.Count(resource =>
                IsResourceVisible(resource) && IsDuplicate(resource.AssetPath));
            var contentDuplicateCount = bundle.Resources.Count(resource =>
                IsResourceVisible(resource) && IsContentDuplicate(resource.AssetPath));
            var duplicate = duplicateCount > 0;
            var previousBackground = GUI.backgroundColor;

            if (selected)
                GUI.backgroundColor = new Color(0.32f, 0.56f, 0.88f);
            else if (duplicate)
                GUI.backgroundColor = new Color(1f, 0.82f, 0.45f);

            var label = FormatBundleLabel(bundle);
            var tooltip = duplicate
                ? $"同对象重复 {exactDuplicateCount} 个，内容重复 {contentDuplicateCount} 个，共 {duplicateCount} 个。"
                : null;
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
                var indirectResources = resources.Where(resource => resource.IsIndirect).ToList();
                var writtenResources = resources.Where(resource => resource.IsWritten).ToList();
                var directCount = selectedBundle.Resources.Count(resource =>
                    resource.IsDirect && IsResourceVisible(resource));
                var indirectCount = selectedBundle.Resources.Count(resource =>
                    resource.IsIndirect && IsResourceVisible(resource));
                var writtenCount = selectedBundle.Resources.Count(resource =>
                    resource.IsWritten && IsResourceVisible(resource));
                DrawResourceSection("直接资源", directResources, directCount);
                GUILayout.Space(8f);
                DrawResourceSection("间接资源", indirectResources, indirectCount);
                if (_viewIndex != 0)
                {
                    GUILayout.Space(8f);
                    DrawResourceSection("写入资源", writtenResources, writtenCount);
                }

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
            var duplicateBundles = GetDuplicateBundles(resource.AssetPath);
            var contentDuplicateBundles = GetContentDuplicateBundles(resource.AssetPath);
            var duplicate = (_viewIndex != 0 || resource.IsDirect) &&
                            (duplicateBundles != null || contentDuplicateBundles != null);
            var previousBackground = GUI.backgroundColor;
            if (duplicate) GUI.backgroundColor = new Color(1f, 0.82f, 0.45f);

            var resourceBundles = GetResourceLocations(resource.AssetPath);
            var bundleInfo = resourceBundles == null || resourceBundles.Count == 0
                ? "\n所在 AB：未记录"
                : $"\n所在 AB：\n{string.Join("\n", resourceBundles)}";
            var referencingFiles = resource.ReferencingFiles.Length == 0
                ? string.Empty
                : $"\n由以下直接资源引用:\n{string.Join("\n", resource.ReferencingFiles)}";
            var duplicateWrittenBundles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (duplicateBundles != null) duplicateWrittenBundles.UnionWith(duplicateBundles);
            if (contentDuplicateBundles != null) duplicateWrittenBundles.UnionWith(contentDuplicateBundles);

            string tooltip;
            if (duplicate && duplicateWrittenBundles.Count > 0)
            {
                var duplicateBundleText = string.Join("\n", duplicateWrittenBundles
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase));
                tooltip = $"重复写入 AB：\n{duplicateBundleText}";
            }
            else
            {
                tooltip = resource.AssetPath + bundleInfo + referencingFiles;
            }
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
                (!_showDuplicatesOnly || bundle.Resources.Any(resource =>
                    IsResourceVisible(resource) && IsAnyDuplicate(resource.AssetPath))) &&
                (!_showMultipleDirectResourcesOnly || GetVisibleDirectResourceCount(bundle) > 1) &&
                (string.IsNullOrWhiteSpace(_searchText) || BundleMatchesSearch(bundle)))
                .ToList();
        }

        private List<ResourceViewData> GetVisibleResources(BundleViewData bundle)
        {
            var searchMatchesBundle = string.IsNullOrWhiteSpace(_searchText) ||
                ContainsIgnoreCase(bundle.Name, _searchText);
            return bundle.Resources.Where(resource =>
                    IsResourceVisible(resource) &&
                    (!_showDuplicatesOnly || IsAnyDuplicate(resource.AssetPath)) &&
                    (searchMatchesBundle || ContainsIgnoreCase(resource.AssetPath, _searchText)))
                .ToList();
        }

        private bool BundleMatchesSearch(BundleViewData bundle)
        {
            if (ContainsIgnoreCase(bundle.Name, _searchText)) return true;
            return bundle.Resources.Any(resource =>
                IsResourceVisible(resource) && ContainsIgnoreCase(resource.AssetPath, _searchText));
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
            return FindDuplicateBundles(_resourceBundles, assetPath) != null;
        }

        private bool IsContentDuplicate(string assetPath)
        {
            return FindDuplicateBundles(_contentResourceBundles, assetPath) != null;
        }

        private bool IsAnyDuplicate(string assetPath)
        {
            return IsDuplicate(assetPath) || IsContentDuplicate(assetPath);
        }

        private List<string> GetDuplicateBundles(string assetPath)
        {
            return FindDuplicateBundles(_resourceBundles, assetPath);
        }

        private List<string> GetContentDuplicateBundles(string assetPath)
        {
            return FindDuplicateBundles(_contentResourceBundles, assetPath);
        }

        private static List<string> FindDuplicateBundles(
            Dictionary<string, List<string>> bundlesByPath, string assetPath)
        {
            return bundlesByPath.TryGetValue(assetPath, out var bundles) && bundles.Count > 1
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
            _contentResourceBundles.Clear();
            _resourceLocations.Clear();
            _resourceCategories.Clear();
            _viewMessage = null;

            if (_viewIndex == 0)
                LoadConfiguredBuilds();
            else
                LoadBuildLayout(_viewIndex == 1 ? _lastBuildReport : _lastAnalysisReport);

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
                    RegisterResource(resource, bundle.Name);
                    if (resource.IsDirect)
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
                        string.Equals(dependencyPath, directPath, StringComparison.OrdinalIgnoreCase)) continue;

                    AddReferencingFile(referencingFiles, dependencyPath, directPath);
                    if (!directPathSet.Contains(dependencyPath)) dependencyPaths.Add(dependencyPath);
                }
            }

            resources.AddRange(directPaths.Select(path => CreateResourceViewData(
                path, ResourceDisplayKind.Direct, false, GetReferencingFiles(referencingFiles, path))));
            resources.AddRange(dependencyPaths
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .Select(path => CreateResourceViewData(
                    path, ResourceDisplayKind.Indirect, false, GetReferencingFiles(referencingFiles, path))));
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

        private string FormatBundleLabel(BundleViewData bundle)
        {
            var directCount = GetVisibleDirectResourceCount(bundle);
            var counts = new List<string>(2);
            if (directCount > 1) counts.Add(directCount.ToString());

            return counts.Count == 0
                ? bundle.Name
                : $"{bundle.Name} ({string.Join(", ", counts)})";
        }

        private int GetVisibleDirectResourceCount(BundleViewData bundle)
        {
            return bundle.Resources.Count(resource => resource.IsDirect && IsResourceVisible(resource));
        }

        private static ResourceViewData CreateResourceViewData(string assetPath, ResourceDisplayKind displayKind,
            bool isDuplicateInLayout, string[] referencingFiles, int duplicateObjectCount = 0,
            long duplicateObjectBytes = 0, bool isContentDuplicate = false,
            int contentDuplicateObjectCount = 0, long contentDuplicateBytes = 0,
            string[] duplicateBundles = null, string[] contentDuplicateBundles = null,
            long serializedSize = 0)
        {
            return new ResourceViewData(assetPath, displayKind, isDuplicateInLayout, referencingFiles,
                GetResourceCategory(assetPath), duplicateObjectCount, duplicateObjectBytes,
                isContentDuplicate, contentDuplicateObjectCount, contentDuplicateBytes,
                duplicateBundles, contentDuplicateBundles, serializedSize);
        }

        private static void AddDependencyKinds(List<ResourceViewData> resources)
        {
            var resourceMap = resources.ToDictionary(resource => resource.AssetPath,
                StringComparer.OrdinalIgnoreCase);
            var directPaths = resources.Where(resource => resource.IsDirect)
                .Select(resource => resource.AssetPath)
                .ToArray();

            foreach (var directPath in directPaths)
            {
                var dependencies = AssetDatabase.GetDependencies(directPath, true);
                foreach (var dependency in dependencies ?? Array.Empty<string>())
                {
                    var dependencyPath = NormalizeAssetPath(dependency);
                    if (string.IsNullOrWhiteSpace(dependencyPath) ||
                        string.Equals(dependencyPath, directPath, StringComparison.OrdinalIgnoreCase) ||
                        IsGeneratedAssetPath(dependencyPath)) continue;

                    if (!resourceMap.TryGetValue(dependencyPath, out var resource))
                    {
                        resource = CreateResourceViewData(dependencyPath, ResourceDisplayKind.Indirect,
                            false, new[] { directPath });
                        resourceMap.Add(dependencyPath, resource);
                        resources.Add(resource);
                    }
                    else
                    {
                        resource.AddKind(ResourceDisplayKind.Indirect);
                        resource.AddReferencingFile(directPath);
                    }
                }
            }
        }

        private static bool IsGeneratedAssetPath(string path)
        {
            return path != null &&
                   (path.StartsWith("temp:/", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("archive:/", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("library:/", StringComparison.OrdinalIgnoreCase));
        }

        private static ResourceCategory GetResourceCategory(string assetPath)
        {
            var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (mainAssetType == typeof(MonoScript)) return ResourceCategory.Code;
            if (mainAssetType == typeof(Shader) || mainAssetType == typeof(ComputeShader))
                return ResourceCategory.Shader;

            switch (Path.GetExtension(assetPath).ToLowerInvariant())
            {
                case ".cs":
                case ".js":
                case ".boo":
                case ".asmdef":
                case ".asmref":
                case ".dll":
                case ".winmd":
                    return ResourceCategory.Code;
                case ".shader":
                case ".compute":
                case ".shadergraph":
                case ".cginc":
                case ".hlsl":
                case ".hlslinc":
                    return ResourceCategory.Shader;
                default:
                    return ResourceCategory.Other;
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
                    .Where(resource => !string.IsNullOrWhiteSpace(resource.assetPath) &&
                                       !IsGeneratedAssetPath(resource.assetPath))
                    .Select(resource => CreateResourceViewData(
                        NormalizeAssetPath(resource.assetPath),
                        resource.isDirect
                            ? ResourceDisplayKind.Direct | ResourceDisplayKind.Written
                            : ResourceDisplayKind.Written,
                        resource.isDuplicate,
                        resource.referencingFiles ?? Array.Empty<string>(), resource.duplicateObjectCount,
                        resource.duplicateObjectBytes, resource.isContentDuplicate,
                        resource.contentDuplicateObjectCount, resource.contentDuplicateBytes,
                        resource.duplicateBundles, resource.contentDuplicateBundles, resource.serializedSize))
                    .ToList();
                AddDependencyKinds(resources);
                var bundle = new BundleViewData(bundleLayout.name ?? "<未命名 AB>", resources.ToArray());
                _bundles.Add(bundle);

                foreach (var resource in resources)
                {
                    RegisterResource(resource, bundle.Name);
                    if (resource.IsDuplicateInLayout)
                        AddBundleReference(resource.AssetPath, bundle.Name);
                    if (resource.IsContentDuplicate)
                        AddContentBundleReferences(resource.AssetPath, resource.ContentDuplicateBundles);
                }
            }
        }

        private void RegisterResource(ResourceViewData resource, string bundleName)
        {
            _resourceCategories[resource.AssetPath] = resource.Category;
            AddResourceLocation(resource.AssetPath, bundleName);
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

        private void AddContentBundleReferences(string assetPath, IEnumerable<string> bundleNames)
        {
            if (!_contentResourceBundles.TryGetValue(assetPath, out var bundles))
            {
                bundles = new List<string>();
                _contentResourceBundles.Add(assetPath, bundles);
            }

            foreach (var bundleName in bundleNames ?? Array.Empty<string>())
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
            var dirtyScenePath = AssetBundleBuilderUtility.GetDirtyActiveScenePath();
            if (string.IsNullOrEmpty(dirtyScenePath)) return true;

            var message = $"当前场景存在未保存修改：\n\n{dirtyScenePath}\n\n模拟分析前需要保存当前场景，是否继续？";
            if (!EditorUtility.DisplayDialog("模拟分析需要保存当前场景", message, "保存并继续", "取消"))
                return false;

            return EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
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

        [Flags]
        private enum ResourceDisplayKind
        {
            None = 0,
            Direct = 1,
            Indirect = 2,
            Written = 4
        }

        private enum ResourceCategory
        {
            Other,
            Code,
            Shader
        }

        private sealed class BundleViewData
        {
            public readonly string Name;
            public readonly ResourceViewData[] Resources;

            public BundleViewData(string name, ResourceViewData[] resources)
            {
                Name = name;
                Resources = resources ?? Array.Empty<ResourceViewData>();
            }
        }

        private sealed class ResourceViewData
        {
            public readonly string AssetPath;
            public ResourceDisplayKind DisplayKind;
            public readonly bool IsDuplicateInLayout;
            public string[] ReferencingFiles;
            public readonly ResourceCategory Category;
            public readonly int DuplicateObjectCount;
            public readonly long DuplicateObjectBytes;
            public readonly bool IsContentDuplicate;
            public readonly int ContentDuplicateObjectCount;
            public readonly long ContentDuplicateBytes;
            public readonly string[] DuplicateBundles;
            public readonly string[] ContentDuplicateBundles;
            public readonly long SerializedSize;
            public bool IsDirect => (DisplayKind & ResourceDisplayKind.Direct) != 0;
            public bool IsIndirect => (DisplayKind & ResourceDisplayKind.Indirect) != 0;
            public bool IsWritten => (DisplayKind & ResourceDisplayKind.Written) != 0;
            public bool IsCode => Category == ResourceCategory.Code;
            public bool IsShader => Category == ResourceCategory.Shader;

            public ResourceViewData(string assetPath, ResourceDisplayKind displayKind, bool isDuplicateInLayout,
                string[] referencingFiles, ResourceCategory category, int duplicateObjectCount,
                long duplicateObjectBytes, bool isContentDuplicate, int contentDuplicateObjectCount,
                long contentDuplicateBytes, string[] duplicateBundles, string[] contentDuplicateBundles,
                long serializedSize)
            {
                AssetPath = assetPath;
                DisplayKind = displayKind;
                IsDuplicateInLayout = isDuplicateInLayout;
                ReferencingFiles = referencingFiles ?? Array.Empty<string>();
                Category = category;
                DuplicateObjectCount = duplicateObjectCount;
                DuplicateObjectBytes = duplicateObjectBytes;
                IsContentDuplicate = isContentDuplicate;
                ContentDuplicateObjectCount = contentDuplicateObjectCount;
                ContentDuplicateBytes = contentDuplicateBytes;
                DuplicateBundles = duplicateBundles ?? Array.Empty<string>();
                ContentDuplicateBundles = contentDuplicateBundles ?? Array.Empty<string>();
                SerializedSize = serializedSize;
            }

            public void AddKind(ResourceDisplayKind kind)
            {
                DisplayKind |= kind;
            }

            public void AddReferencingFile(string path)
            {
                if (string.IsNullOrWhiteSpace(path) || ReferencingFiles.Contains(path,
                        StringComparer.OrdinalIgnoreCase)) return;

                ReferencingFiles = ReferencingFiles.Concat(new[] { path }).ToArray();
            }
        }
    }
}
