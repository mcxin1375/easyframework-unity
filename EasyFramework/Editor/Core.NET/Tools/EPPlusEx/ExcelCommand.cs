//using EasyFramework.Excel.Scripts;
//using EasyFramework.Lib;
//using OfficeOpenXml;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Linq;

//namespace EasyFramework.Excel
//{
//    public static class ExcelCommand
//    {
//        private static HashSet<string> ExcelExs { get; } = new HashSet<string>()
//        {
//            ".xls",
//            ".xlsx",
//        };

//        static ExcelCommand()
//        {
//            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
//        }

//        public static async Task ExecuteAsync(ParseExcelSettings settings, Action<string, int, int> progressAction = null)
//        {
//            await ExecuteAsync(settings, progressAction, CancellationToken.None);
//        }
//        public static async Task ExecuteAsync(ParseExcelSettings settings, Action<string, int, int> progressAction, CancellationToken token)
//        {
//            if (!Directory.Exists(settings.DataPath))
//            {
//                Console.WriteLine($"DataPath Not Exists: {settings.DataPath}");
//                progressAction?.Invoke($"DataPath Not Exists: {settings.DataPath}", 0, 0);
//                return;
//            }

//            int svnRevesion = 0;
//            if (!string.IsNullOrWhiteSpace(settings.SvnVersionPath))
//            {
//                try
//                {
//                    var svnInfo = await SVNCommand.Info(settings.SvnVersionPath);
//                    svnRevesion = svnInfo?.Revision ?? 0;
//                }
//                catch { }
//            }

//            string[] files = Directory.GetFiles(settings.DataPath, "*.*", SearchOption.AllDirectories);
//            List<ExcelInfo> excelList = new List<ExcelInfo>();
//            int progress = 0;

//            StringBuilder stringBuilder = new StringBuilder();

//            using (MemoryStream memoryStream = new MemoryStream())
//            using (BinaryWriter writer = new BinaryWriter(memoryStream))
//            {
//                writer.Write(svnRevesion);

//                foreach (string excelFile in files)
//                {
//                    if (token.IsCancellationRequested) return;

//                    //Console.WriteLine(excelFile);

//                    progress++;
//                    string fileName = Path.GetFileName(excelFile);
//                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(excelFile);
//                    if (fileName.StartsWith("~$"))
//                    {
//                        progressAction?.Invoke($"Skip: {excelFile}", progress, files.Length);
//                        continue;
//                    }
//                    string ex = Path.GetExtension(excelFile);
//                    if (!ExcelExs.Contains(ex))
//                    {
//                        progressAction?.Invoke($"Skip: {excelFile}", progress, files.Length);
//                        continue;
//                    }

//                    using (ExcelPackage package = new ExcelPackage(new FileInfo(excelFile)))
//                    {
//                        foreach (ExcelWorksheet? worksheet in package.Workbook.Worksheets)
//                        {
//                            if (worksheet == null || worksheet.Dimension == null) continue;
//                            if (fileNameWithoutExtension != worksheet.Name) continue;
//                            //if (settings.IgnoreSheetNameHash.Contains(worksheet.Name)) continue;

//                            int rowCount = worksheet.Dimension.Rows;
//                            int colCount = worksheet.Dimension.Columns;

//                            if (rowCount < settings.DataRow) continue;
//                            List<int> keyIndexList = new List<int>();
//                            List<string> keyValueList = new List<string>();
//                            List<string> typeValueList = new List<string>();

//                            for (int i = 1; i <= colCount; i++)
//                            {
//                                if (settings.UseTypeRow > 0)
//                                {
//                                    var useTypeValue = worksheet.Cells[settings.UseTypeRow, i].Value?.ToString();
//                                    if (string.IsNullOrWhiteSpace(useTypeValue) || !settings.UseTypeHash.Contains(useTypeValue)) continue;
//                                }

//                                var keyValue = worksheet.Cells[settings.KeyRow, i].Value?.ToString();
//                                var typeValue = worksheet.Cells[settings.TypeRow, i].Value?.ToString();

//                                if (string.IsNullOrWhiteSpace(keyValue))
//                                {
//                                    //throw new Exception($"[{keyRow}, {i}] KeyValue Is Null! SheetName: {sheet.Name} File: {file}");
//                                    continue;
//                                }
//                                if (string.IsNullOrWhiteSpace(typeValue))
//                                {
//                                    //throw new Exception($"[{typeRow}, {i}] TypeValue Is Null! SheetName: {sheet.Name} File: {file}");
//                                    continue;
//                                }
//                                keyValue = keyValue.Trim();
//                                typeValue = typeValue.Trim();

//                                if (ParseExcelSettings.TypeChangeDict.ContainsKey(typeValue))
//                                {
//                                    typeValue = ParseExcelSettings.TypeChangeDict[typeValue];
//                                }
//                                if (!ParseExcelSettings.SupportTypes.Contains(typeValue)) continue;

//                                keyIndexList.Add(i);
//                                keyValueList.Add(keyValue);
//                                typeValueList.Add(typeValue);
//                                //Console.WriteLine($"keyIndex: {i}, keyValue: {keyValue}");
//                            }
//                            if (keyIndexList.Count == 0) continue;

//                            string excelName = $"{settings.FirstName}{worksheet.Name}{settings.EndName}";
//                            int dataLen = 0;
//                            for (int i = settings.DataRow; i <= rowCount; i++)
//                            {
//                                var cellKeyStr = worksheet.Cells[i, keyIndexList[0]].Value?.ToString();
//                                if (string.IsNullOrWhiteSpace(cellKeyStr)) continue;
//                                dataLen++;
//                            }

//                            //int dataLen = rowCount - settings.DataRow + 1;
//                            writer.Write(dataLen);
//                            stringBuilder.AppendLine($"{excelName} : {dataLen}");

//                            for (int i = settings.DataRow; i <= rowCount; i++)
//                            {
//                                var rowIndex = i;

//                                var cellKeyStr = worksheet.Cells[rowIndex, keyIndexList[0]].Value?.ToString();
//                                if (string.IsNullOrWhiteSpace(cellKeyStr)) continue;

//                                StringBuilder sb = new StringBuilder();
//                                sb.Append($"{cellKeyStr}: ");
//                                for (int j = 0; j < keyIndexList.Count; j++)
//                                {
//                                    var keyIndex = keyIndexList[j];
//                                    var keyValue = keyValueList[j];
//                                    var typeValue = typeValueList[j];

//                                    var cell = worksheet.Cells[rowIndex, keyIndex].Value;
//                                    var cellStr = worksheet.Cells[rowIndex, keyIndex].Value?.ToString();

//                                    if (j == 0) sb.Append(cellStr);
//                                    else sb.Append($", {cellStr}");

//                                    try
//                                    {
//                                        switch (typeValue)
//                                        {
//                                            case "bool":
//                                                bool boolValue = Convert.ToBoolean(cell);
//                                                writer.Write(boolValue);
//                                                break;
//                                            case "byte":
//                                                byte byteValue = Convert.ToByte(cell);
//                                                writer.Write(byteValue);
//                                                break;
//                                            case "short":
//                                                short shortValue = Convert.ToInt16(cell);
//                                                writer.Write(shortValue);
//                                                break;
//                                            case "int":
//                                                int intValue = Convert.ToInt32(cell);
//                                                writer.Write(intValue);
//                                                break;
//                                            case "long":
//                                                long longValue = Convert.ToInt64(cell);
//                                                writer.Write(longValue);
//                                                break;
//                                            case "ushort":
//                                                ushort ushortValue = Convert.ToUInt16(cell);
//                                                writer.Write(ushortValue);
//                                                break;
//                                            case "uint":
//                                                uint uintValue = Convert.ToUInt32(cell);
//                                                writer.Write(uintValue);
//                                                break;
//                                            case "ulong":
//                                                ulong ulongValue = Convert.ToUInt64(cell);
//                                                writer.Write(ulongValue);
//                                                break;
//                                            case "double":
//                                                double doubleValue = Convert.ToDouble(cell);
//                                                writer.Write(doubleValue);
//                                                break;
//                                            case "float":
//                                                float floatValue = Convert.ToSingle(cell);
//                                                writer.Write(floatValue);
//                                                break;
//                                            case "enum":
//                                                int enumValue = Convert.ToInt32(cell);
//                                                writer.Write(enumValue);
//                                                break;
//                                            default:
//                                                string value = Convert.ToString(cell) ?? string.Empty;
//                                                writer.Write(value);
//                                                break;
//                                        }
//                                    }
//                                    catch
//                                    {
//                                        switch (typeValue)
//                                        {
//                                            case "bool":
//                                                if (!bool.TryParse(cellStr, out bool boolValue)) boolValue = cellStr == "1";
//                                                writer.Write(boolValue);
//                                                break;
//                                            case "byte":
//                                                if (!byte.TryParse(cellStr, out byte byteValue)) byteValue = 0;
//                                                writer.Write(byteValue);
//                                                break;
//                                            case "short":
//                                                if (!short.TryParse(cellStr, out short shortValue)) shortValue = 0;
//                                                writer.Write(shortValue);
//                                                break;
//                                            case "int":
//                                                if (!int.TryParse(cellStr, out int intValue)) intValue = 0;
//                                                writer.Write(intValue);
//                                                break;
//                                            case "long":
//                                                if (!long.TryParse(cellStr, out long longValue)) longValue = 0;
//                                                writer.Write(longValue);
//                                                break;
//                                            case "ushort":
//                                                if (!ushort.TryParse(cellStr, out ushort ushortValue)) ushortValue = 0;
//                                                writer.Write(ushortValue);
//                                                break;
//                                            case "uint":
//                                                if (!uint.TryParse(cellStr, out uint uintValue)) uintValue = 0;
//                                                writer.Write(uintValue);
//                                                break;
//                                            case "ulong":
//                                                if (!ulong.TryParse(cellStr, out ulong ulongValue)) ulongValue = 0;
//                                                writer.Write(ulongValue);
//                                                break;
//                                            case "double":
//                                                if (!double.TryParse(cellStr, out double doubleValue)) doubleValue = 0;
//                                                writer.Write(doubleValue);
//                                                break;
//                                            case "float":
//                                                if (!float.TryParse(cellStr, out float floatValue)) floatValue = 0;
//                                                writer.Write(floatValue);
//                                                break;
//                                            case "enum":
//                                                if (!int.TryParse(cellStr, out int enumValue)) enumValue = 0;
//                                                writer.Write(enumValue);
//                                                break;
//                                            default:
//                                                string value = cellStr ?? string.Empty;
//                                                writer.Write(value);
//                                                break;
//                                        }
//                                    }

//                                }
//                                //Console.WriteLine($"row({rowIndex}) {str}");

//                                stringBuilder.AppendLine(sb.ToString());
//                            }

//                            excelList.Add(new ExcelInfo(excelName, keyValueList.ToArray(), typeValueList.ToArray()));

//                            progressAction?.Invoke($"{excelFile}  {worksheet.Name}", progress, files.Length);
//                        }
//                    }
//                }

//                if (!string.IsNullOrWhiteSpace(settings.OutputDataFilePath))
//                {
//                    string dir = Path.GetDirectoryName(settings.OutputDataFilePath);
//                    Directory.CreateDirectory(dir);
//                    File.WriteAllBytes(settings.OutputDataFilePath, memoryStream.ToArray());
//                }
//                if (!string.IsNullOrWhiteSpace(settings.OutputTxtFilePath))
//                {
//                    string dir = Path.GetDirectoryName(settings.OutputTxtFilePath);
//                    Directory.CreateDirectory(dir);
//                    File.WriteAllText(settings.OutputTxtFilePath, stringBuilder.ToString());
//                }
//                if (!string.IsNullOrWhiteSpace(settings.OutputScriptPath))
//                {
//                    Directory.CreateDirectory(settings.OutputScriptPath);
//                    //EasyHelper.ClearDirectory(settings.CSharpOutputPath);

//                    foreach (var excelOutputInfo in excelList)
//                    {
//                        string csFile = $"{settings.OutputScriptPath}/{excelOutputInfo.Name}.cs";
//                        File.WriteAllText(csFile, excelOutputInfo.ToCSharp());
//                    }

//                    string template = @"
//using System.IO;

//namespace NAMESPACE_NAME
//{
//    public static class ExcelDataLoader
//    {
//        public static int SvnRevision { get; private set; }

//        public static void Load(byte[] binary)
//        {
//            using MemoryStream memoryStream = new MemoryStream(binary);
//            using BinaryReader reader = new BinaryReader(memoryStream);
//            Load(reader);
//        }

//        public static void Load(string file)
//        {
//            using FileStream fileStream = new FileStream(file, FileMode.Open);
//            using BinaryReader reader = new BinaryReader(fileStream);
//            Load(reader);
//        }
        
//        private static void Load(BinaryReader reader)
//        {
//                SvnRevision = reader.ReadInt32();
////LOAD_DATA
//        }
//    }
//}
//";
//                    StringBuilder sb = new StringBuilder();
//                    foreach (var excelInfo in excelList)
//                    {
//                        sb.AppendLine($"                {excelInfo.Name}.InitInternal(reader);");
//                    }
//                    string content = template.Replace("//LOAD_DATA", sb.ToString())
//                                             .Replace("NAMESPACE_NAME", ParseExcelSettings.NamespaceName);
//                    string loaderFile = $"{settings.OutputScriptPath}/ExcelDataLoader.cs";
//                    File.WriteAllText(loaderFile, content);

//                }
//            }
//        }
//    }
//}
