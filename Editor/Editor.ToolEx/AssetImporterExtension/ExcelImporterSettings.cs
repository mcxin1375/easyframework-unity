/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{

    [CreateAssetMenu(menuName = "EasyFramework/AssetImporter/ExcelImporterSettings", fileName = "ExcelImporterSettings.asset")]
    public class ExcelImporterSettings : ScriptableObject, IAssetImporterExtension
    {
        public int Order => order;

        public bool enabled = true;
        public int order;
        
        [Header("Settings")]
        public string namespaceName = "Game";
        public string dataPath = "../Data/Config";
        public string svnVersionPath = "../Data/Config";
        public string outputScriptPath = "Assets/Scripts/Game/ExcelData";
        public string outputDataFilePath = "Assets/Res_DLC/DLC/Config/ExcelData.bytes";
        
        public void OnExecute()
        {
            if (!enabled) return;
            
            ExcelCommandSettings excelCommandSettings = new()
            {
                namespaceName = namespaceName,
                dataPath = dataPath,
                svnVersionPath = $"{Application.dataPath}/../{svnVersionPath}",
                outputScriptPath = outputScriptPath,
                outputDataFilePath = outputDataFilePath,
            };
            
            ExcelCommand.Execute(excelCommandSettings, (s, p, len) =>
            {
                Debug.Log($"({p}/{len}) {s}");
                EditorUtility.DisplayProgressBar("ExcelImporter", s, p / (float)len);
            });

            EditorUtility.ClearProgressBar();
        }
    }
}