/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2024/5/8
// describe:
//----------------------------------------------------------------*/

using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace EasyFramework.Editor
{
    public interface IDataBuilderExtension : IEditorToolExtension
    {
        void OnExecuteBefore();
        void OnExecuteAfter();
    }

    public class DataBuilder : EditorTool<DataBuilder, IDataBuilderExtension>
    {
        [MenuItem("EasyFramework/Tools/DataBuilder - Build", priority = EasyFrameworkToolsSettings.DataBuilder)]
        public static void MenuItem() => DataBuilder.Instance.Build();

        public void Build()
        {
            foreach (var extension in Extensions) extension.OnExecuteBefore();
            
            var outputPath = ProjectDataPath;
            FileHelper.CreateDirectory(outputPath);
            FileHelper.ClearDirectory(outputPath);

            var dataFiles = GetDataFiles();
            foreach (var file in dataFiles)
            {
                string fileName = Path.GetFileName(file);
                string writeFile = $"{outputPath}/{fileName}";
                
                var bytes = File.ReadAllBytes(file);
                File.WriteAllBytes(writeFile, bytes);
            }
            
            foreach (var extension in Extensions) extension.OnExecuteAfter();
        }

        public string[] GetDataFiles()
        {
            List<string> fileList = new();
            var settings = DataBuilderSettings.Instance;
            if (settings.buildDirectories?.Length > 0)
            {
                foreach (var buildDirectory in settings.buildDirectories)
                {
                    if (!Directory.Exists(buildDirectory)) continue;

                    foreach (var ex in settings.buildFileExes)
                    {
                        string[] files = Directory.GetFiles(buildDirectory, $"*{ex}", SearchOption.AllDirectories);
                        if (files.Length > 0) fileList.AddRange(files);
                    }
                }
            }
            return fileList.ToArray();
        }

        public string[] GetBuildFiles()
        {
            return Directory.Exists(ProjectDataPath) ? Directory.GetFiles(ProjectDataPath, "*", SearchOption.AllDirectories) : null;
        }
    }
}