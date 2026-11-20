/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2023/6/23
// describe:
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EasyFramework.Editor
{
    public static class SVCCollectorUtility
    {
        private const string TemporaryFilePrefix = "SVCCollectorTemp_";

        public static void SaveCurrentSVC()
        {
            var settings = SVCCollectorSettings.Instance;

            FileHelper.CreateDirectory(settings.svcSaveDirectory);
            CleanupTemporaryFiles(settings.svcSaveDirectory);
            string temporaryFile = CreateTemporaryFilePath(settings.svcSaveDirectory);

            try
            {
                // Unity's internal exporter replaces its destination, so export first and merge into the saved asset.
                SaveCurrentShaderVariantCollection(temporaryFile);
                var currentCollection = ImportCollection(temporaryFile);
                MergeIntoSavedCollection(temporaryFile, settings.SvcSaveFile, currentCollection);
            }
            finally
            {
                DeleteTemporaryFile(temporaryFile);
                AssetDatabase.Refresh();
            }
        }

        private static string CreateTemporaryFilePath(string directory)
        {
            directory = directory.Replace('\\', '/').TrimEnd('/');
            return $"{directory}/{TemporaryFilePrefix}{System.Guid.NewGuid():N}.shadervariants";
        }

        private static void CleanupTemporaryFiles(string directory)
        {
            if (!Directory.Exists(directory)) return;

            foreach (string file in Directory.GetFiles(directory, $"{TemporaryFilePrefix}*.shadervariants"))
                DeleteTemporaryFile(file);
        }

        private static void SaveCurrentShaderVariantCollection(string path)
        {
            var method = typeof(ShaderUtil).GetMethod(
                "SaveCurrentShaderVariantCollection",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null) throw new MissingMethodException("ShaderUtil.SaveCurrentShaderVariantCollection was not found.");

            method.Invoke(null, new object[] { path });
        }

        private static ShaderVariantCollection LoadSavedCollection(string path)
        {
            var collection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(path);
            if (collection != null || !File.Exists(path)) return collection;

            return ImportCollection(path);
        }

        private static ShaderVariantCollection ImportCollection(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var collection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(path);
            if (collection == null)
                throw new InvalidOperationException($"Failed to import shader variant collection: {path}");

            return collection;
        }

        private static void MergeIntoSavedCollection(string temporaryFile, string saveFile,
            ShaderVariantCollection currentCollection)
        {
            var savedCollection = LoadSavedCollection(saveFile);
            if (savedCollection == null)
            {
                CreateSavedCollection(temporaryFile, saveFile);
                return;
            }

            if (AddNewVariants(savedCollection, currentCollection) == 0) return;
            EditorUtility.SetDirty(savedCollection);
            AssetDatabase.SaveAssetIfDirty(savedCollection);
        }

        private static void CreateSavedCollection(string temporaryFile, string saveFile)
        {
            if (!AssetDatabase.CopyAsset(temporaryFile, saveFile))
                throw new InvalidOperationException($"Failed to create shader variant collection: {saveFile}");

            var collection = ImportCollection(saveFile);
            collection.name = Path.GetFileNameWithoutExtension(saveFile);
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssetIfDirty(collection);
        }

        private static void DeleteTemporaryFile(string path)
        {
            AssetDatabase.DeleteAsset(path.Replace('\\', '/'));
            FileHelper.DeleteFile(path);
            FileHelper.DeleteFile($"{path}.meta");
        }

        private static int AddNewVariants(ShaderVariantCollection target, ShaderVariantCollection source)
        {
            int addedVariantCount = 0;
            foreach (var variant in SVCHelper.GetShaderVariants(source))
            {
                if (target.Add(variant)) addedVariantCount++;
            }

            return addedVariantCount;
        }

        // public void CommitSVCFile()
        // {
        //     var settings = SVCCollectorSettings.Instance;
        //     var arr = new string[]
        //     {
        //         settings.SvcSaveFile,
        //         $"{settings.SvcSaveFile}.meta"
        //     };
        //     SVNCommand.CommitAll(arr, "", (str) =>
        //     {
        //         Debug.Log(str);
        //     });
        // }

        public static SVCInfo CreateShaderVariantCollectionInfo()
        {
            var settings = SVCCollectorSettings.Instance;
            List<ShaderVariantCollection.ShaderVariant> tmpList = new();
            foreach (string dir in settings.svcDirectories)
            {
                if (!Directory.Exists(dir)) continue;
                
                string[] files = Directory.GetFiles(dir, "*.shadervariants", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    ShaderVariantCollection collection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(file);
                    var variants = SVCHelper.GetShaderVariants(collection);
                    foreach (var variant in variants)
                    {
                        tmpList.Add(variant);
                    }
                }
            }

            if (settings.shaderDirectories?.Length > 0)
            {
                var list = UnityEditorHelper.FindAssets<Shader>(settings.shaderDirectories);
                return new SVCInfo(tmpList.ToArray(), list);
            }
            return new SVCInfo(tmpList.ToArray());
        }
    }
}
