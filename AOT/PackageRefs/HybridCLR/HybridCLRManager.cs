/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

#if EASYFRAMEWORK_HYBRIDCLR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HybridCLR;
using UnityEngine;

namespace EasyFramework
{
    public class HybridCLRManager : Singleton<HybridCLRManager>
    {
        public const string FileExtension = ".bytes";
        
        public EState State { get; private set; }
        
        public enum EResult
        {
            Success,
            UpdateVersionError,
            DownloadError,
            LoadError,
        }
        public enum EState
        {
            None,
            UpdateVersion,
            Downloading,
            Loading,
            Completed,
        }

        public enum ELoadType
        {
            Dll,
            MetaData
        }

        private readonly List<Assembly> _assemblies = new();
        private HybridCLRBuilderVersion _versionInfo;
        private Action<EResult> _callback = null;
        
        public void Enter(Action<EResult> callback = null)
        {
            if (State != EState.None || State == EState.Completed)
            {
                Debug.LogError($"HybridCLRManager is busying, current state: {State}");
                return;
            }

            _callback = callback;
            EnterState(EState.UpdateVersion);
        }

        public void EnterEditor(Action<EResult> callback = null)
        {
            if (State != EState.None || State == EState.Completed)
            {
                Debug.LogError($"HybridCLRManager is busying, current state: {State}");
                return;
            }
            
            _callback = callback;
#if UNITY_EDITOR
            HybridCLRLoadingEditor();
#endif
        }

        public async ETask<Assembly> LoadAsync(string assemblyName, ELoadType loadType = ELoadType.Dll)
        {
#if UNITY_EDITOR
            Assembly assemblyEditor = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
            EnterLogic(assemblyEditor);
            return assemblyEditor;
#endif
            
            var result = await FAOT.DLCManager.DownloadFileAsync(assemblyName);
            if (result)
            {
                switch (loadType)
                {
                    case ELoadType.Dll:
                        
                        var dllFileName = $"{assemblyName}{FileExtension}";
                        var dllPdbFileName = $"{assemblyName}.pdb{FileExtension}";

                        var dllData = FAOT.LocalStorageManager.ReadAllBytes(dllFileName, ELocalStorageType.DLC);
                        var dllPdbData = FAOT.LocalStorageManager.ReadAllBytes(dllPdbFileName, ELocalStorageType.DLC);
                        
                        var assembly = LoadDll(dllData, dllPdbData);
                        _assemblies.Add(assembly);
                        
                        EnterLogic(assembly);

                        return assembly;
                    case ELoadType.MetaData:
                        var metaDataFileName = assemblyName.EndsWith(FileExtension)
                            ? assemblyName
                            : $"{assemblyName}{FileExtension}";
                        LoadMetaData(FAOT.LocalStorageManager.ReadAllBytes(metaDataFileName, ELocalStorageType.DLC));
                        
                        break;
                }
            }
            return null;
        }

        private void OnCompleted(EResult result)
        {
            State = EState.Completed;
            _callback?.Invoke(result);
            _callback = null;
        }

        private void EnterState(EState state)
        {
            State = state;
            switch (state)
            {
                case EState.UpdateVersion:
                    UpdateVersion();
                    break;
                case EState.Downloading:
                    DownloadFiles();
                    break;
                case EState.Loading:
#if UNITY_EDITOR
                    HybridCLRLoadingEditor();
#else

                    HybridCLRLoading();
#endif
                    break;
            }
        }

        private void UpdateVersion()
        {
            if (FAOT.LocalStorageManager.Exists(HybridCLRBuilderVersion.FileName, ELocalStorageType.DLC))
            {
                _versionInfo = FAOT.LocalStorageManager.LoadObject<HybridCLRBuilderVersion>(HybridCLRBuilderVersion.FileName, ELocalStorageType.DLC);
                EnterState(EState.Loading);
                return;
            }
            
            FDebug.Log($"更新版本：{HybridCLRBuilderVersion.FileName}");
            FAOT.DLCManager.DownloadFile(HybridCLRBuilderVersion.FileName, b =>
            {
                FDebug.Log($"更新版本：{HybridCLRBuilderVersion.FileName}, {b}");
                if (b)
                {
                    _versionInfo = FAOT.LocalStorageManager.LoadObject<HybridCLRBuilderVersion>(HybridCLRBuilderVersion.FileName, ELocalStorageType.DLC);
                    EnterState(EState.Downloading);
                }
                else
                {
                    OnCompleted(EResult.UpdateVersionError);
                }
            });
        }

        private void DownloadFiles()
        {
            List<string> tmpList = new();
            if (_versionInfo.stripDlls?.Length > 0)
            {
                foreach (var stripDll in _versionInfo.stripDlls)
                {
                    var fileName = $"{stripDll}{FileExtension}";
                    tmpList.Add(fileName);
                }
            }
            if (_versionInfo.allDlls?.Length > 0)
            {
                foreach (var dllName in _versionInfo.allDlls)
                {
                    var fileName = $"{dllName}{FileExtension}";
                    var pdbFileName = $"{dllName}.pdb{FileExtension}";
                    tmpList.Add(fileName);
                    tmpList.Add(pdbFileName);
                }
            }
            FAOT.DLCManager.DownloadFiles(tmpList.ToArray(), b =>
            {
                if (b)
                {
                    EnterState(EState.Loading);
                }
                else
                {
                    OnCompleted(EResult.DownloadError);
                }
            });
        }

        private void HybridCLRLoading()
        {
            try
            {
                if (_versionInfo.stripDlls?.Length > 0)
                {
                    for (int i = 0; i < _versionInfo.stripDlls.Length; i++)
                    {
                        var stripDll = _versionInfo.stripDlls[i];
                        var fileName = $"{stripDll}{FileExtension}";

                        LoadMetaData(FAOT.LocalStorageManager.ReadAllBytes(fileName, ELocalStorageType.DLC));
                    }
                }
                if (_versionInfo.loadDlls?.Length > 0)
                {
                    for (int i = 0; i < _versionInfo.loadDlls.Length; i++)
                    {
                        var dllName = _versionInfo.loadDlls[i];
                        var fileName = $"{dllName}{FileExtension}";
                        var pdbFileName = $"{dllName}.pdb{FileExtension}";

                        var dllData = FAOT.LocalStorageManager.ReadAllBytes(fileName, ELocalStorageType.DLC);
                        var dllPdbData = FAOT.LocalStorageManager.ReadAllBytes(pdbFileName, ELocalStorageType.DLC);
                        _assemblies.Add(LoadDll(dllData, dllPdbData));
                    }
                }
                foreach (var assembly in _assemblies)
                {
                    EnterLogic(assembly);
                }
                OnCompleted(EResult.Success);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                OnCompleted(EResult.LoadError);
            }
        }

#if UNITY_EDITOR
        private void HybridCLRLoadingEditor()
        {
            try
            {
                var info = EasyFrameworkAOTEditorBridge.DllVersion;
                if (info == null)
                {
                    Debug.LogError($"EasyFrameworkAOTEditorBridge.DllVersion is null");
                    return;
                }

                if (info.loadDlls?.Length > 0)
                {
                    foreach (var dllName in info.loadDlls)
                    {
                        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == dllName);
                        _assemblies.Add(assembly);
                    }
                }

                foreach (var assembly in _assemblies)
                {
                    EnterLogic(assembly);
                }
                OnCompleted(EResult.Success);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                OnCompleted(EResult.LoadError);
            }
        }
#endif

        private LoadImageErrorCode LoadMetaData(string metaDataFile)
        {
            Debug.Log($"[HybridCLR - LoadMetaData] metaDataFile: {metaDataFile}");
            byte[] bytes = File.ReadAllBytes(metaDataFile);
            return HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(bytes, HybridCLR.HomologousImageMode.SuperSet);
        }
        private LoadImageErrorCode LoadMetaData(byte[] data)
        {   
            return HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(data, HybridCLR.HomologousImageMode.SuperSet);
        }

        private Assembly LoadDll(string dllFile, string pdbFile)
        {
            Debug.Log($"[HybridCLR - LoadDll] DllFile: {dllFile}, PdbFile: {pdbFile}");
            return File.Exists(pdbFile)
                ? Assembly.Load(File.ReadAllBytes(dllFile), File.ReadAllBytes(pdbFile))
                : Assembly.Load(File.ReadAllBytes(dllFile));
        }
        private Assembly LoadDll(byte[] dllData, byte[] pdbData)
        {
            return pdbData != null
                ? Assembly.Load(dllData, pdbData)
                : Assembly.Load(dllData);
        }

        private void EnterLogic(Assembly assembly)
        {
            var enterType = EasyFrameworkAOTSettings.Instance.enterType;
            var enterMethod = EasyFrameworkAOTSettings.Instance.enterMethod;

            string typeFullName = $"{assembly.GetName().Name}.{enterType}";
            var type = assembly.GetType(typeFullName) ?? assembly.GetType(enterType);
            var method = type?.GetMethod(enterMethod);
            if (method != null)
            {
                Debug.Log($"[HybridCLR - EnterLogic] Assembly: {assembly.GetName().Name} Type: {type}  Method: {method}");
                method.Invoke(null, null);
            }
        }
    }
}

#endif