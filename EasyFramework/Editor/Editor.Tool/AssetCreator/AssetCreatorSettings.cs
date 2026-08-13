/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using UnityEngine;

namespace EasyFramework.Editor
{
    public abstract class AssetCreatorScriptable : ScriptableObject, IAssetCreatorExtension
    {
        public int Order => order;
        [Header("Base Settings")]
        public bool enabled = true;
        public int order;
        public abstract void OnExecute();
    }

    [ProjectSettings("ProjectSettings/EasyFramework")]
    public class AssetCreatorSettings : ProjectSettingsEditor<AssetCreatorSettings>
    {
        
    }
}