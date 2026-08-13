/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

namespace EasyFramework.Editor
{
    // public enum EncryptType
    // {
    //     None,
    //     DES,
    //     AES,
    //     XOR
    // }
    // [Serializable]
    // public class DataBuilderInfo
    // {
    //     public EncryptType encryptType = EncryptType.None;
    //     public string secretKey = "";
    //     public string[] sourceDirectories;
    // }
    
    [ProjectSettings("ProjectSettings/EasyFramework")]
    public class DataBuilderSettings : ProjectSettingsEditor<DataBuilderSettings>
    {
        public string[] buildDirectories = new string[]
        {
            "Assets/Res_DLC"
        };
        public string[] buildFileExes = new string[]
        {
            ".json",
            ".txt",
            ".bytes",
        };

    }
}