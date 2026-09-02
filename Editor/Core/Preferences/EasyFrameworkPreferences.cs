using System;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public class EasyFrameworkPreferences
    {
        public const string SettingPath = "Preferences/Easy Framework";
        public const string GithubURL = @"https://github.com/mcxin1375/easyframework-unity";

        private static Settings _settings;
        internal static Settings s_Settings
        {
            get
            {
                if (_settings == null) LoadSettings();
                return _settings;
            }
        }

        internal const string kSettingPath = "ProjectSettings/EasyFramework/EasyFrameworkPreferences.json";

        internal static void LoadSettings()
        {
            _settings = ConfigHelper.LoadOrCreate<Settings>(kSettingPath);
        }

        internal static void SaveSettings()
        {
            ConfigHelper.Save(s_Settings, kSettingPath, true);
        }

        private class GUIScope : GUI.Scope
        {
            float m_LabelWidth;

            public GUIScope(float layoutMaxWidth)
            {
                m_LabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 250;
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUILayout.BeginVertical();
                GUILayout.Space(15);
            }

            public GUIScope() : this(500)
            {
            }

            protected override void CloseScope()
            {
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = m_LabelWidth;
            }
        }

#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        static SettingsProvider CreateBuildCacheProvider()
        {
            var provider = new SettingsProvider(SettingPath, SettingsScope.User, SettingsProvider.GetSearchKeywordsFromGUIContentProperties<Properties>());
            provider.guiHandler = sarchContext => OnGUI();
            return provider;
        }

#else
        [PreferenceItem("Scriptable Build Pipeline")]
#endif
        static void OnGUI()
        {
            using (new GUIScope())
            {
                EditorGUI.BeginChangeCheck();
                DrawProperties();
                if (EditorGUI.EndChangeCheck())
                    SaveSettings();
            }
        }

        public static string AssetsDataPath => s_Settings.AssetsDataPath;
        public static string AssetsDataDLCPath => s_Settings.AssetsDataDLCPath;
        public static string AssetsDataResourcesPath => s_Settings.AssetsDataResourcesPath;
        
        public static string LanAddress => s_Settings.LanAddress;
        public static string ServerUrl => s_Settings.ServerUrl;

        public static string ProjectDataPath
        {
            get => s_Settings.ProjectDataPath;
            internal set => CompareAndSet(ref s_Settings.ProjectDataPath, value);
        }

        public static string ProjectName
        {
            get => s_Settings.ProjectName;
            set => CompareAndSet(ref s_Settings.ProjectName, value);
        }

        public static string ProjectFullPath => $"{Application.dataPath}/../";

        [Serializable]
        internal class Settings
        {
            public string ProjectDataPath = "EasyFramework";
            public string AssetsDataPath = "Assets/EasyFrameworkData";
            
            public string AssetsDataDLCPath = "Assets/EasyFrameworkData/DLC";
            public string AssetsDataResourcesPath = "Assets/EasyFrameworkData/Resources";
            
            public string LanAddress = "//ClientWorkPro/WorkSpace";
            public string ServerUrl = "https://192.168.3.127:7000";

            public string ProjectName = "EasyFrameworkProject";
        }

        static void DrawProperties()
        {
            s_Settings.ProjectName = EditorGUILayout.TextField("ProjectName", s_Settings.ProjectName);
            s_Settings.ProjectDataPath = EditorGUILayout.TextField("ProjectDataPath", s_Settings.ProjectDataPath);
            s_Settings.AssetsDataPath = EditorGUILayout.TextField("AssetsDataPath", s_Settings.AssetsDataPath);
            
            s_Settings.AssetsDataDLCPath = EditorGUILayout.TextField("AssetsDataDLCPath", s_Settings.AssetsDataDLCPath);
            s_Settings.AssetsDataResourcesPath = EditorGUILayout.TextField("AssetsDataResourcesPath", s_Settings.AssetsDataResourcesPath);

            s_Settings.LanAddress = EditorGUILayout.TextField("LanAddress", s_Settings.LanAddress);
            s_Settings.ServerUrl = EditorGUILayout.TextField("ServerUrl", s_Settings.ServerUrl);
        }

        static void CompareAndSet<T>(ref T property, T value)
        {
            if (property.Equals(value))
                return;

            property = value;
            SaveSettings();
        }

        internal class Properties
        {
            public static readonly GUIContent frameworkSettings = EditorGUIUtility.TrTextContent("Framework Settings");
            public static readonly GUIContent projectSettings = EditorGUIUtility.TrTextContent("Project Settings");
        }
    }
}