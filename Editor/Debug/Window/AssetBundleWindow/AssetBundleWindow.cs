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

        private Vector2 _bundleScrollPosition;
        private Vector2 _resourceScrollPosition;
        private string _searchText = string.Empty;
        private string _selectedBundleName;
        private string _refreshError;
        private bool _showDuplicatesOnly;
        private bool _hasLoaded;

        private int DuplicateResourceCount => _resourceBundles.Count(pair => pair.Value.Count > 1);
        private int DuplicateReferenceCount => _resourceBundles
            .Where(pair => pair.Value.Count > 1)
            .Sum(pair => pair.Value.Count);
        private int TotalResourceCount => _bundles.Sum(bundle => bundle.AssetPaths.Length);

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
                EditorGUILayout.HelpBox("正在读取 AssetBundleBuilder 配置...", MessageType.Info);
                return;
            }

            DrawSummary();

            if (!string.IsNullOrEmpty(_refreshError))
            {
                EditorGUILayout.HelpBox(_refreshError, MessageType.Error);
                return;
            }

            var visibleBundles = GetVisibleBundles();
            EnsureSelectedBundle(visibleBundles);

            if (DuplicateResourceCount > 0)
            {
                EditorGUILayout.HelpBox(
                    $"发现 {DuplicateResourceCount} 个资源被打包到多个 AB 包，共 {DuplicateReferenceCount} 次引用。黄色行表示重复资源。",
                    MessageType.Warning);
            }

            DrawContent(visibleBundles);
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
                {
                    SettingsService.OpenProjectSettings("Project/EasyFramework Tools/AssetBundleBuilder");
                }

                GUILayout.Space(6f);
                var duplicateFilter = GUILayout.Toggle(
                    _showDuplicatesOnly,
                    "仅看重复资源",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(92f));
                if (duplicateFilter != _showDuplicatesOnly)
                {
                    _showDuplicatesOnly = duplicateFilter;
                    _bundleScrollPosition = Vector2.zero;
                    _resourceScrollPosition = Vector2.zero;
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label("搜索", EditorStyles.miniLabel, GUILayout.Width(32f));
                var searchFieldStyle = GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.toolbarTextField;
                var searchText = EditorGUILayout.TextField(_searchText, searchFieldStyle,
                    GUILayout.MinWidth(160f), GUILayout.MaxWidth(360f));
                if (!string.Equals(searchText, _searchText, StringComparison.Ordinal))
                {
                    _searchText = searchText;
                    _bundleScrollPosition = Vector2.zero;
                    _resourceScrollPosition = Vector2.zero;
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

        private void DrawSummary()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                DrawSummaryItem("AB 包", _bundles.Count.ToString());
                DrawSummaryItem("资源引用", TotalResourceCount.ToString());
                DrawSummaryItem("重复资源", DuplicateResourceCount.ToString(), DuplicateResourceCount > 0);
                DrawSummaryItem("重复引用", DuplicateReferenceCount.ToString(), DuplicateReferenceCount > 0);

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
                    EditorGUILayout.HelpBox("没有匹配的 AB 包。", MessageType.Info);
                }
                else
                {
                    foreach (var bundle in visibleBundles)
                    {
                        DrawBundleRow(bundle);
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawBundleRow(BundleViewData bundle)
        {
            var selected = string.Equals(bundle.Name, _selectedBundleName, StringComparison.OrdinalIgnoreCase);
            var duplicate = bundle.DuplicateResourceCount > 0;
            var previousBackground = GUI.backgroundColor;

            if (selected)
                GUI.backgroundColor = new Color(0.32f, 0.56f, 0.88f);
            else if (duplicate)
                GUI.backgroundColor = new Color(1f, 0.82f, 0.45f);

            var label = duplicate
                ? $"!  {bundle.Name}   ({bundle.AssetPaths.Length}, 重复 {bundle.DuplicateResourceCount})"
                : $"    {bundle.Name}   ({bundle.AssetPaths.Length})";
            if (GUILayout.Button(new GUIContent(label, duplicate ? "该 AB 包包含重复资源" : null),
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
                EditorGUILayout.LabelField(
                    $"资源列表 ({GetVisibleResourcePaths(selectedBundle).Count}/{selectedBundle.AssetPaths.Length})",
                    EditorStyles.miniLabel);

                DrawResourceHeader();
                _resourceScrollPosition = EditorGUILayout.BeginScrollView(_resourceScrollPosition);
                var resourcePaths = GetVisibleResourcePaths(selectedBundle);
                if (resourcePaths.Count == 0)
                {
                    EditorGUILayout.HelpBox("没有匹配的资源。", MessageType.Info);
                }
                else
                {
                    foreach (var assetPath in resourcePaths)
                    {
                        DrawResourceRow(assetPath);
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private static void DrawResourceHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("资源路径", EditorStyles.miniLabel);
                GUILayout.Label("状态", EditorStyles.miniLabel, GUILayout.Width(145f));
                GUILayout.Label(string.Empty, GUILayout.Width(44f));
            }
        }

        private void DrawResourceRow(string assetPath)
        {
            var duplicateBundles = GetDuplicateBundles(assetPath);
            var duplicate = duplicateBundles != null;
            var previousBackground = GUI.backgroundColor;
            if (duplicate) GUI.backgroundColor = new Color(1f, 0.82f, 0.45f);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.Height(RowHeight)))
            {
                GUILayout.Label(new GUIContent(assetPath, assetPath), GUILayout.ExpandWidth(true));
                if (duplicate)
                {
                    GUILayout.Label($"重复 x{duplicateBundles.Count}", EditorStyles.boldLabel,
                        GUILayout.Width(74f));
                    var duplicateBundleText = string.Join(", ", duplicateBundles);
                    GUILayout.Label(new GUIContent(duplicateBundleText, duplicateBundleText), EditorStyles.miniLabel,
                        GUILayout.Width(70f));
                }
                else
                {
                    GUILayout.Label("正常", EditorStyles.miniLabel, GUILayout.Width(145f));
                }

                if (GUILayout.Button("定位", EditorStyles.miniButton, GUILayout.Width(42f)))
                {
                    var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    if (asset != null)
                    {
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }
                    else
                    {
                        Debug.LogWarning($"AssetBundleWindow: 无法定位资源 {assetPath}");
                    }
                }
            }

            GUI.backgroundColor = previousBackground;
        }

        private List<BundleViewData> GetVisibleBundles()
        {
            return _bundles.Where(bundle =>
                (!_showDuplicatesOnly || bundle.DuplicateResourceCount > 0) &&
                (string.IsNullOrWhiteSpace(_searchText) || BundleMatchesSearch(bundle)))
                .ToList();
        }

        private List<string> GetVisibleResourcePaths(BundleViewData bundle)
        {
            var searchMatchesBundle = string.IsNullOrWhiteSpace(_searchText) ||
                ContainsIgnoreCase(bundle.Name, _searchText);
            return bundle.AssetPaths.Where(path =>
                (!_showDuplicatesOnly || IsDuplicate(path)) &&
                (searchMatchesBundle || ContainsIgnoreCase(path, _searchText)))
                .ToList();
        }

        private bool BundleMatchesSearch(BundleViewData bundle)
        {
            if (ContainsIgnoreCase(bundle.Name, _searchText)) return true;
            return bundle.AssetPaths.Any(path => ContainsIgnoreCase(path, _searchText));
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
            return _resourceBundles.TryGetValue(assetPath, out var bundles) && bundles.Count > 1 ? bundles : null;
        }

        private void RefreshData()
        {
            _hasLoaded = false;
            _refreshError = null;
            _bundles.Clear();
            _resourceBundles.Clear();

            try
            {
                ToolExtension<IAssetBundleBuilderSettings>.Refresh();
                var builds = AssetBundleBuilderUtility.CreateAssetBundleBuildBySettings() ??
                    Array.Empty<AssetBundleBuild>();
                var resourceBundleMap = new Dictionary<string, HashSet<string>>(
                    StringComparer.OrdinalIgnoreCase);

                foreach (var build in builds.OrderBy(build => build.assetBundleName, StringComparer.OrdinalIgnoreCase))
                {
                    var assetPaths = (build.assetNames ?? Array.Empty<string>())
                        .Where(path => !string.IsNullOrWhiteSpace(path))
                        .Select(NormalizeAssetPath)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();
                    var bundle = new BundleViewData(build.assetBundleName ?? "<未命名 AB>", assetPaths);
                    _bundles.Add(bundle);

                    foreach (var assetPath in assetPaths)
                    {
                        if (!resourceBundleMap.TryGetValue(assetPath, out var bundleNames))
                        {
                            bundleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            resourceBundleMap.Add(assetPath, bundleNames);
                        }

                        bundleNames.Add(bundle.Name);
                    }
                }

                foreach (var pair in resourceBundleMap)
                {
                    _resourceBundles[pair.Key] = pair.Value.OrderBy(name => name,
                        StringComparer.OrdinalIgnoreCase).ToList();
                }

                foreach (var bundle in _bundles)
                {
                    bundle.DuplicateResourceCount = bundle.AssetPaths.Count(IsDuplicate);
                }

                EnsureSelectedBundle(GetVisibleBundles());
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

        private static string NormalizeAssetPath(string path)
        {
            return path.Replace('\\', '/');
        }

        private sealed class BundleViewData
        {
            public readonly string Name;
            public readonly string[] AssetPaths;
            public int DuplicateResourceCount;

            public BundleViewData(string name, string[] assetPaths)
            {
                Name = name;
                AssetPaths = assetPaths ?? Array.Empty<string>();
            }
        }
    }
}
