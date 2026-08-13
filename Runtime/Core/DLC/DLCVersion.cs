// /*----------------------------------------------------------------
// // author:Cookie(mcx)
// // date:2024/5/8
// // describe:
// //----------------------------------------------------------------*/
//
// using System;
// using Newtonsoft.Json;
//
// namespace EasyFramework.AOT
// {
//     public enum DLCResMode
//     {
//         DLCList,
//         DLCZip,
//         All
//     }
//     
//     [Serializable]
//     public class DLCVersion
//     {
//         public int mainVersion;
//         public string uid;
//         public string name;
//         public string[] packages;
//         public string[] forcedPackages;
//         // public int version;
//         // public int assetBuilderVer;
//         // public string assetBuilderResStr;
//         // public string buildTime;
//
//         public DLCResMode resMode;
//         public ToolVersion dlcBuilderVersion = new();
//         public ToolVersion assetBundleBuilderVersion = new();
//         public ToolVersion dllBuilderVersion = new();
//         public ToolVersion dataBuilderVersion = new();
//         
//         [JsonIgnore] public string VersionName => string.IsNullOrEmpty(name) ? Version.ToString() : name;
//         [JsonIgnore] public int Version => dlcBuilderVersion.version;
//         [JsonIgnore] public int SvnRevision => dlcBuilderVersion.revision;
//         [JsonIgnore] public string AssetBuilderSvnRevisionStr => $"{assetBundleBuilderVersion.revision}.{dllBuilderVersion.revision}.{dataBuilderVersion.revision}";
//         public string PackageToString() => string.Join(",", packages);
//     }
// }