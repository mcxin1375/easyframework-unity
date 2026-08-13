/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public static class SVCCollectorPlayMode
    {
        private static bool _initialized = false;
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            
            // Debug.Log("ShaderVariantExtension Initializing");
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            var settings = SVCCollectorSettings.Instance;
            if (!settings.svcEnabled) return;
            
            if (settings.svcSavePlayState == obj)
            {
                SVCCollector.Instance.Execute();
            }
        }
    }

    public interface ISVCCollectorExtension : IEditorToolExtension
    {
        void OnExecuteBefore() { }
        void OnExecuteAfter() { }
    }

    public class SVCCollector : EditorTool<SVCCollector, ISVCCollectorExtension>
    {

        // [MenuItem("EasyFramework/Tools/SVCCollector - SaveCurrentSVC", priority = EasyFrameworkToolsSettings.SVCCollector)]
        // private static void MenuItem1()
        // {
        //     Instance.Execute();
        // }

        public void Execute()
        {
            foreach (var extension in Extensions) extension.OnExecuteBefore();
            
            SaveCurrentSVC();
            
            var settings = SVCCollectorSettings.Instance;
            if (settings.svnCommitEnabled)
            {
                CommitSVCFile();
            }
            
            foreach (var extension in Extensions) extension.OnExecuteAfter();
        }

        public void SaveCurrentSVC()
        {
            var settings = SVCCollectorSettings.Instance;
            
            FileHelper.CreateDirectory(settings.svcSaveDirectory);
            typeof(ShaderUtil).GetMethod("SaveCurrentShaderVariantCollection", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { settings.SvcSaveFile });
        
            AssetDatabase.Refresh();
        }

        public void CommitSVCFile()
        {
            var settings = SVCCollectorSettings.Instance;
            var arr = new string[]
            {
                settings.SvcSaveFile,
                $"{settings.SvcSaveFile}.meta"
            };
            SVNCommand.CommitAll(arr, "", (str) =>
            {
                Debug.Log(str);
            });
        }

        public SVCInfo CreateShaderVariantCollectionInfo()
        {
            var settings = SVCCollectorSettings.Instance;
            List<ShaderVariantCollection.ShaderVariant> tmpList = new();
            foreach (string dir in settings.svcDirectories)
            {
                if (!Directory.Exists(dir)) continue;
                
                string[] files = Directory.GetFiles(dir, "*.shadervariants", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    ShaderVariantCollection collection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(file);
                    var variants = SVCHelper.GetShaderVariants(collection);
                    foreach (var variant in variants)
                    {
                        tmpList.Add(variant);
                    }
                }
            }

            if (settings.shaderDirectories?.Length > 0)
            {
                var list = UnityEditorHelper.FindAssets<Shader>(settings.shaderDirectories);
                return new SVCInfo(tmpList.ToArray(), list);
            }
            return new SVCInfo(tmpList.ToArray());
        }
        
    }
}