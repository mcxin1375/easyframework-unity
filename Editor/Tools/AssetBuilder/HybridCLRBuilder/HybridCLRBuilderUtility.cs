/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

#if EF_HYBRIDCLR

using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace EasyFramework.Editor
{
    public static class HybridCLRBuilderUtility
    {
        public static void BuildBySettings(string outputPath)
        {
            FileHelper.CreateDirectory(outputPath);
            FileHelper.ClearDirectory(outputPath);

            var dllDir = HybridCLR.Editor.SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget);
            var dllStripDir = HybridCLR.Editor.SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget);

            var info = CreateHybridCLRVersionInfo();
            if (info.allDlls?.Length > 0)
            {
                foreach (string dll in info.allDlls)
                {
                    string fromFile = $"{dllDir}/{dll}.dll";
                    string toFile = $"{outputPath}/{dll}{HybridCLRManager.FileExtension}";
                    File.Copy(fromFile, toFile, true);
                
                    string fromPdbFile = $"{dllDir}/{dll}.pdb";
                    string toPdbFile = $"{outputPath}/{dll}.pdb{HybridCLRManager.FileExtension}";
                    File.Copy(fromPdbFile, toPdbFile, true);
                }
            }

            if (info.stripDlls?.Length > 0)
            {
                foreach (var stripDll in info.stripDlls)
                {
                    string fromFile = $"{dllStripDir}/{stripDll}.dll";
                    string toFile = $"{outputPath}/{stripDll}{HybridCLRManager.FileExtension}";
                    File.Copy(fromFile, toFile, true);
                }
            }

            info.SetToolVersion(HybridCLRBuilder.Instance.Version);
            ConfigHelper.Save(info, $"{outputPath}/{HybridCLRBuilderVersion.FileName}", true);
        }

        public static HybridCLRBuilderVersion CreateHybridCLRVersionInfo()
        {
            var info = new HybridCLRBuilderVersion();
            info.stripDlls = HybridCLRBuilderSettings.Instance.stripDlls;
            info.allDlls = GetHotUpdateDlls();
            info.loadDlls = HybridCLRBuilderSettings.Instance.loadAll
                ? info.allDlls
                : HybridCLRBuilderSettings.Instance.customLoadDlls;
            return info;
        }

        private static string[] GetHotUpdateDlls()
        {
            List<string> dllList = new List<string>();
            
            var hybridSettings = HybridCLR.Editor.Settings.HybridCLRSettings.Instance;
            if (hybridSettings.hotUpdateAssemblies?.Length > 0)
            {
                foreach (var definition in hybridSettings.hotUpdateAssemblies) dllList.Add(definition);
            }
            if (hybridSettings.hotUpdateAssemblyDefinitions?.Length > 0)
            {
                foreach (var definition in hybridSettings.hotUpdateAssemblyDefinitions) dllList.Add(definition.name);
            }

            return dllList.ToArray();
        }
        
    }
}

#endif