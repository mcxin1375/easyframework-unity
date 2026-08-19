/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using UnityEngine;

namespace EasyFramework.Editor
{

    [CreateAssetMenu(menuName = "EasyFramework/AssetImporter/ResImporterSettings", fileName = "ResImporterSettings.asset")]
    public class ResImporterSettingsSO : ToolScriptableObject<ToolImporter>
    {
        [Header("Settings")]
        public ResImporterConfig[] resImporterConfigs;
        
        [Serializable]
        public class ResImporterConfig
        {
            public string from;
            public string to;
            public bool deleteDiff = true;
        }
        
        protected override void OnExecute()
        {
            foreach (var importRes in resImporterConfigs)
            {
                // var from = $"{Application.dataPath}/{settings.rootPath}/{importRes.From}";
                // var to = $"{Application.dataPath}/{settings.rootPath}/{importRes.To}";
                var from = importRes.from;
                var to = importRes.to;
            
                Debug.Log($"ImportRes: {importRes.from} >> {importRes.to}");
            
                FileHelper.CopyDirectory(from, to, true, importRes.deleteDiff, (s, p, len) => {
                    if (!string.IsNullOrEmpty(s))
                        Debug.Log($"({p}/{len}) {s}");
                });
            
                FileHelper.DeleteNotExistsMeta(from, to, (s) => {
                    Debug.Log($"Del: {s}");
                });
            }
        }
    }
}