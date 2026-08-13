/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{

    [CreateAssetMenu(menuName = "EasyFramework/AssetImporter/ProtocImporterSettings", fileName = "ProtocImporterSettings.asset")]
    public class ProtocImporterSettings : ScriptableObject, IAssetImporterExtension
    {
        public const string ProtocExeGuid = "4772bbc776eedce4bba1d4ab1b72bfbb";
        
        
        public int Order => order;

        public bool enabled = true;
        public int order;
        
        [Header("Settings")]
        public ProtocCommandSettings protocCommandSettings = new()
        {
            dataPath = "../../Data/Protoc",
            svnVersionPath = "../../Data/Protoc",
            outputProtocPath = "../../Client/RGProject/Assets/Scripts/Network/Protoc",
            outputProxyPath = "../../Client/RGProject/Assets/Scripts/Network/Proxy",
        };
        
        public void OnExecute()
        {
            if (!enabled) return;
            
            string guidPath = AssetDatabase.GUIDToAssetPath(ProtocExeGuid);
            if (!File.Exists(guidPath)) throw new Exception($"Not exists: {guidPath}");
            string protocFile = $"{Application.dataPath}/../{guidPath}";
            protocCommandSettings.protocExeFile = protocFile;
            
            ProtocCommand.Execute(protocCommandSettings, (str) => {
                Debug.Log(str);
            });
        }
    }
}