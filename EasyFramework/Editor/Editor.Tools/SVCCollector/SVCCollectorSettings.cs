/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    [ProjectSettings("ProjectSettings/EasyFramework")]
    public class SVCCollectorSettings : ProjectSettingsEditor<SVCCollectorSettings>
    {
        [Header("AssetBundleBuilder Extension")] 
        public bool preprocessShaders = true;
        
        [Header("SVC Location")] 
        public string[] svcDirectories = new string[] { "Assets/Res/SVC" };
        public string[] shaderDirectories;
        
        [Header("SVC PlayMode Settings")] 
        public bool svcEnabled = true;
        public bool svnCommitEnabled;
        public PlayModeStateChange svcSavePlayState = PlayModeStateChange.ExitingPlayMode;
        
        [Header("SVC Save Settings")] 
        public string svcSaveDirectory = "Assets/Res/SVC";
        public string SvcSaveFile => $"{svcSaveDirectory}/{SvcFileName}.shadervariants";
        public string SvcFileName
        {
            get
            {
                var val = EditorPrefs.GetString("SvcFileName");
                if (string.IsNullOrEmpty(val)) return System.Environment.UserName;
                return val;
            }
            set
            {
                EditorPrefs.SetString("SvcFileName", value);
            }
        }
    }
}