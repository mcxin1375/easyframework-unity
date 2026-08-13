using System;
using System.Text;

namespace EasyFramework.Editor
{
    public static class ExcelCodeGenerator
    {
        
        private const string ExcelLoaderTemplate = @"
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace NAMESPACE_NAME
{
    public class EXCEL_LOADER
    {
        public readonly int SvnRevision;

//DATA_ITEMS
//DATA_DICT
        private EXCEL_LOADER(BinaryReader reader)
        {
            SvnRevision = reader.ReadInt32();
//LOAD_ITEMS
//LOAD_DICT
        }
//DATA_GET
        public static EXCEL_LOADER Load(byte[] binary)
        {
            using MemoryStream memoryStream = new MemoryStream(binary);
            using BinaryReader reader = new BinaryReader(memoryStream);
            return new EXCEL_LOADER(reader);
        }
        public static EXCEL_LOADER Load(string file)
        {
            using FileStream fileStream = new FileStream(file, FileMode.Open);
            using BinaryReader reader = new BinaryReader(fileStream);
            return new EXCEL_LOADER(reader);
        }
    }
//EXCEL_CLASS
}
";

        private const string ExcelDataReaderTemplate = @"
    public partial class ExcelName
    {
        internal static ExcelName[] Load(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            var arr = new ExcelName[count];
            for (int i = 0; i < count; i++) arr[i] = new ExcelName(reader);
            return arr;
        }
        private ExcelName(BinaryReader reader)
        {
//EXCEL_READER        }
    }
";
        public static string GenerateExcelDataCode(ExcelInfo[] excelInfos, string namespaceName = "EasyFramework", string configTypeName = "ExcelData")
        {
            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            StringBuilder sb4 = new StringBuilder();
            StringBuilder sb5 = new StringBuilder();
            StringBuilder sb6 = new StringBuilder();
            foreach (var excelInfo in excelInfos)
            {
                sb1.AppendLine($"        public readonly {excelInfo.TypeName}[] {excelInfo.TableName}Items;");
                sb2.AppendLine($"        private readonly Dictionary<{excelInfo.KeyType}, {excelInfo.TypeName}> _{excelInfo.TableName}Dict;");
                sb3.AppendLine($"            {excelInfo.TableName}Items = {excelInfo.TypeName}.Load(reader);");
                sb4.AppendLine($"            _{excelInfo.TableName}Dict = {excelInfo.TableName}Items.ToDictionary(item => item.{excelInfo.KeyValue}, item => item);");
                sb5.AppendLine($"        public {excelInfo.TypeName} Get{excelInfo.TableName}({excelInfo.KeyType} key) => _{excelInfo.TableName}Dict.ContainsKey(key) ? _{excelInfo.TableName}Dict[key] : null;");
                
                StringBuilder temp = new();
                foreach (var info in excelInfo.Heads) temp.AppendLine($"            {info.Key} = {TypeToReader(info.WriteType, info.Type)}");
                sb6.Append(ExcelDataReaderTemplate.Replace("ExcelName", excelInfo.TypeName).Replace("//EXCEL_READER", temp.ToString()));
            }
            string content = ExcelLoaderTemplate.Replace("EXCEL_LOADER", configTypeName).Replace("NAMESPACE_NAME", namespaceName);
            content = content.Replace("//DATA_ITEMS", sb1.ToString());
            content = content.Replace("//DATA_DICT", sb2.ToString());
            content = content.Replace("//LOAD_ITEMS", sb3.ToString());
            content = content.Replace("//LOAD_DICT", sb4.ToString());
            content = content.Replace("//DATA_GET", sb5.ToString());
            content = content.Replace("//EXCEL_CLASS", sb6.ToString());
            return content;
        }
        
        private const string NamespaceTemplate = @"
namespace NAMESPACE_NAME
{
//CONTENT
}
";
        public static string GenerateExcelConfigCode(ExcelInfo[] excelInfos, string namespaceName = "EasyFramework")
        {
            StringBuilder sb = new StringBuilder();
            foreach (var excelInfo in excelInfos)
            {
                sb.AppendLine($"    public partial class {excelInfo.TypeName}\n" + "    {");
                foreach (var info in excelInfo.Heads)
                {
                    sb.AppendLine($"        /// <summary>");
                    sb.AppendLine($"        /// {info.Description}");
                    sb.AppendLine($"        /// </summary>");
                    sb.AppendLine($"        public readonly {info.Type} {info.Key}; // {info.Description}");
                }
                sb.AppendLine("    }");
            }
            return NamespaceTemplate.Replace("NAMESPACE_NAME", namespaceName).Replace("//CONTENT", sb.ToString());
        }
        public static string GenerateEnumCode(ExcelEnumInfo[] enumInfos, string namespaceName = "EasyFramework")
        {
            StringBuilder sb = new();
            foreach (var enumInfo in enumInfos)
            {
                sb.AppendLine($"    public enum {enumInfo.TypeName}\n" + "    {");
                foreach (var info in enumInfo.ValueList)
                {
                    sb.AppendLine($"        /// <summary>");
                    sb.AppendLine($"        /// {info.Description}");
                    sb.AppendLine($"        /// </summary>");
                    sb.AppendLine($"        {info.Key} = {info.Value}, // {info.Description}");
                }
                sb.AppendLine("    }");
            }
            return NamespaceTemplate.Replace("NAMESPACE_NAME", namespaceName).Replace("//CONTENT", sb.ToString());
        }

        private static string TypeToReader(string type, string typeName)
        {
            switch (type)
            {
                case "bool": return "reader.ReadBoolean();";
                case "byte": return "reader.ReadByte();";
                case "short": return "reader.ReadInt16();";
                case "int": return "reader.ReadInt32();";
                case "long": return "reader.ReadInt64();";
                case "ushort": return "reader.ReadUInt16();";
                case "uint": return "reader.ReadUInt32();";
                case "ulong": return "reader.ReadUInt64();";
                case "double": return "reader.ReadDouble();";
                case "float": return "reader.ReadSingle();";
                case "string": return "reader.ReadString();";
                case "enum": return $"({typeName})reader.ReadInt32();";
            }
            throw new Exception($"Unknown type {type}");
        }
    }
}

// namespace EasyFrameworkTemplate
// {
//     public enum ExcelType
//     {
//         Value = 0, // Desc
//     }
// }