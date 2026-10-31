/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public static class EasyFrameworkWindowMenuItem
    {
        private const int Priority = MenuItemOrder.Windows;

        [MenuItem("EasyFramework/Windows/ExcelViewerWindow", priority = Priority + 100)]
        private static void ExcelViewerWindow_Open() => ExcelViewerWindow.Open();

        [MenuItem("EasyFramework/Windows/AssetBundleWindow", priority = Priority + 200)]
        private static void AssetBundleWindow_Open() => AssetBundleWindow.Open();
    }
}
