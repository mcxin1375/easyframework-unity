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
    class SVCCollectorExtension : IToolEvent<SVCCollector>
    {
        void IToolEvent<SVCCollector>.OnExecute() => SVCCollector.Instance.ExecuteBySettings();
    }
    
    public partial class SVCCollector
    {
        public void ExecuteBySettings()
        {
            SaveCurrentSVC();
        }
        
        public void SaveCurrentSVC()
        {
            var settings = SVCCollectorSettings.Instance;
            
            FileHelper.CreateDirectory(settings.svcSaveDirectory);
            typeof(ShaderUtil).GetMethod("SaveCurrentShaderVariantCollection", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { settings.SvcSaveFile });
        
            AssetDatabase.Refresh();
        }

        // public void CommitSVCFile()
        // {
        //     var settings = SVCCollectorSettings.Instance;
        //     var arr = new string[]
        //     {
        //         settings.SvcSaveFile,
        //         $"{settings.SvcSaveFile}.meta"
        //     };
        //     SVNCommand.CommitAll(arr, "", (str) =>
        //     {
        //         Debug.Log(str);
        //     });
        // }

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