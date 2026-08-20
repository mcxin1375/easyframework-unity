
using System.IO;
using System.Reflection;

namespace EasyFramework
{
    public static class HybridCLRHelper
    {
#if EF_HYBRIDCLR
        public static HybridCLR.LoadImageErrorCode LoadMetaData(string metaDataFile)
        {
            FDebug.Log($"[HybridCLR - LoadMetaData] metaDataFile: {metaDataFile}");
            byte[] bytes = File.ReadAllBytes(metaDataFile);
            return HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(bytes, HybridCLR.HomologousImageMode.SuperSet);
        }
        public static HybridCLR.LoadImageErrorCode LoadMetaData(byte[] data)
        {   
            return HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(data, HybridCLR.HomologousImageMode.SuperSet);
        }
#endif

        public static Assembly LoadDll(string dllFile, string pdbFile)
        {
            FDebug.Log($"[HybridCLR - LoadDll] DllFile: {dllFile}, PdbFile: {pdbFile}");
            return File.Exists(pdbFile)
                ? Assembly.Load(File.ReadAllBytes(dllFile), File.ReadAllBytes(pdbFile))
                : Assembly.Load(File.ReadAllBytes(dllFile));
        }
        
        public static Assembly LoadDll(byte[] dllData, byte[] pdbData)
        {
            return pdbData != null
                ? Assembly.Load(dllData, pdbData)
                : Assembly.Load(dllData);
        }

        public static void Enter(Assembly assembly)
        {
            var enterType = EasyFrameworkSettings.Instance.enterType;
            var enterMethod = EasyFrameworkSettings.Instance.enterMethod;

            string typeFullName = $"{assembly.GetName().Name}.{enterType}";
            var type = assembly.GetType(typeFullName) ?? assembly.GetType(enterType);
            var method = type?.GetMethod(enterMethod);
            if (method != null)
            {
                FDebug.Log($"[HybridCLR - Enter] Assembly: {assembly.GetName().Name} Type: {type}  Method: {method}");
                method.Invoke(null, null);
            }
        }
    }
}