
using System;
using System.Text;

namespace EasyFramework.Editor
{
    // public enum EPlatform
    // {
    //     Android,
    //     iOS
    // }
    [Flags]
    public enum ETaskResultState
    {
        None = 0,
        Failed = 1, 
        Succeeded = 2, 
        Canceled =4
    }

    public struct TaskConfig
    {
        public string AppName;
        public Platform Platform;
        public string DLCVersion;
        public int Revision;

        // Server
        public bool SVN;
        public bool ResImporterServer;
        public bool ExcelImporterServer;
        public bool ProtocImporterServer;
        public bool FTPServer;

        //public bool AssetEditor;
        public bool ResImporter;
        public bool AssetImporter;
        public bool AssetCreator;
        public bool ExcelImporter;
        public bool ProtocImporter;

        //public bool AssetBuilder;
        public bool AssetBundleBuilder;
        public bool DllBuilder;
        public bool DataBuilder;
        public bool DLCBuilder;

        public bool HotUpdate;
        public bool DevelopmentBuild;
        public bool BuildPlayer;
        public bool BuildProject;

        public bool ForceBuild;

        
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append($"AppName: {AppName}, Platform: {Platform}, DLCVersion: {DLCVersion}, Revision: {Revision}\n");

            if (SVN) sb.Append($"SVN, ");
            if (ResImporterServer) sb.Append($"ResImporterServer, ");
            if (ExcelImporterServer) sb.Append($"ExcelImporterServer, ");
            if (ProtocImporterServer) sb.Append($"ProtocImporterServer, ");
            if (FTPServer) sb.Append($"FTPServer, ");

            if (ResImporter) sb.Append($"ResImporter, ");
            if (AssetImporter) sb.Append($"AssetImporter, ");
            if (AssetCreator) sb.Append($"AssetCreator, ");
            if (ExcelImporter) sb.Append($"ExcelImporter, ");
            if (ProtocImporter) sb.Append($"ProtocImporter, ");

            if (AssetBundleBuilder) sb.Append($"AssetBundleBuilder, ");
            if (DllBuilder) sb.Append($"DllBuilder, ");
            if (DataBuilder) sb.Append($"DataBuilder, ");
            if (DLCBuilder) sb.Append($"DLCBuilder, ");

            if (HotUpdate) sb.Append($"HotUpdate, ");
            if (DevelopmentBuild) sb.Append($"DevelopmentBuild, ");
            if (BuildPlayer) sb.Append($"BuildPlayer, ");
            if (BuildProject) sb.Append($"BuildProject, ");

            return sb.ToString();
        }
        
        public static TaskConfig Default => new TaskConfig()
        {
            SVN = true,
            ResImporterServer = true,
            ExcelImporterServer = true,
            ProtocImporterServer = true,
            HotUpdate = true,
        };

        public static TaskConfig LoadFromFile(string path)
        {
            return NewtonsoftHelper.LoadOrCreate<TaskConfig>(path);
        }
    }
}
