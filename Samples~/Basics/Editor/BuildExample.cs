using EasyFramework.Editor;
using UnityEditor;

namespace EasyFramework.Samples.Basics.Editor
{
    public static class BuildExample
    {
        [MenuItem("EasyFramework/Samples/Build Player")]
        public static void Execute()
        {
            // Default PlayerBuilder.assetBuilder = true runs the resource chain first.
            // Enable DLCBuilder.copyVersionToStreamingAssets for an offline Player.
            PlayerBuilder.Instance.Execute();
        }
    }
}
