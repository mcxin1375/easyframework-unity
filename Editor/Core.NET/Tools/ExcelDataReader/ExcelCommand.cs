using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyFramework.Editor
{
    public static class ExcelCommand
    {
        private static readonly HashSet<string> ExcelExes = new()
        {
            ".xls",
            ".xlsx",
        };
        // private static readonly HashSet<string> IgnoreSheetNameHash = new()
        // {
        //     "Sheet1",
        // };
        private const int DescRow = 1;
        private const int KeyRow = 2;
        private const int TypeRow = 3;
        private const int UseTypeRow = 5;
        private const int DataRow = 6;
        private static readonly HashSet<string> UseTypeHash = new()
        {
            "1",
            "3",
        };
        private static readonly Dictionary<string, string> TypeChangeDict = new()
        {
            ["text"] = "string",
            ["path"] = "string",
            ["enum"] = "int",
        };
        private static readonly HashSet<string> SupportTypes = new()
        {
            "bool",
            "byte",
            "short",
            "int",
            "long",
            "ushort",
            "uint",
            "ulong",
            "double",
            "float",
            "string",
            "enum",
            
            "text",
            "path",
        };
        private static readonly Dictionary<string, Action<BinaryWriter, string>> Writers;

        static ExcelCommand()
        {
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            
            Writers = new Dictionary<string, Action<BinaryWriter, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["enum"]   = (w, s) => w.Write(int.TryParse(s, out var v) ? v : 0),
                ["bool"]   = (w, s) => w.Write(bool.TryParse(s, out var v) ? v : s == "1"),
                ["byte"]   = (w, s) => w.Write(byte.TryParse(s, out var v) ? v : (byte)0),
                ["short"]  = (w, s) => w.Write(short.TryParse(s, out var v) ? v : (short)0),
                ["int"]    = (w, s) => w.Write(int.TryParse(s, out var v) ? v : 0),
                ["long"]   = (w, s) => w.Write(long.TryParse(s, out var v) ? v : 0L),
                ["ushort"] = (w, s) => w.Write(ushort.TryParse(s, out var v) ? v : (ushort)0),
                ["uint"]   = (w, s) => w.Write(uint.TryParse(s, out var v) ? v : 0U),
                ["ulong"]  = (w, s) => w.Write(ulong.TryParse(s, out var v) ? v : 0UL),
                ["double"] = (w, s) => w.Write(double.TryParse(s, out var v) ? v : 0d),
                ["float"]  = (w, s) => w.Write(float.TryParse(s, out var v) ? v : 0f),
                ["string"] = (w, s) => w.Write(s)
            };
        }

        public static void Execute(ExcelCommandSettings settings, Action<string, int, int> progressAction = null)
        {
            if (!Directory.Exists(settings.dataPath)) throw new Exception($"dataPath not exists: {settings.dataPath}");
            // if (!Directory.Exists(settings.outputScriptPath)) throw new Exception($"outputScriptPath not exists: {settings.outputScriptPath}");
            if (string.IsNullOrWhiteSpace(settings.outputDataFilePath)) throw new Exception($"outputDataFilePath is empty : {settings.outputDataFilePath}");
            
            string[] files = GetExcelFiles(settings.dataPath);
            
            using MemoryStream memoryStream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(memoryStream);
            
            SVNCommand.TryGetRevision(settings.svnVersionPath, out var svnRevision);
            writer.Write(svnRevision);

            List<ExcelInfo> excelList = new List<ExcelInfo>();
            Dictionary<string, ExcelEnumInfo> enumDict = new();
            for (int index = 0; index < files.Length; index++)
            {
                var excelFile = files[index];
                ParseExcelFile(excelFile, settings, excelList, enumDict, writer);
                progressAction?.Invoke($"{excelFile}", index + 1, files.Length);
            }

            // if (!string.IsNullOrWhiteSpace(settings.OutputDebugPath) && Directory.Exists(settings.OutputScriptPath))
            // {
            //     var hashSet = excelList.Select(item => $"{item.TypeName}.txt").ToHashSet();
            //         
            //     var deleteArr = Directory.GetFiles(settings.OutputDebugPath);
            //     foreach (var item in deleteArr)
            //     {
            //         var fileName = Path.GetFileName(item);
            //         if (fileName.EndsWith(".meta")) fileName = fileName.Replace(".meta", "");
            //
            //         if (hashSet.Contains(fileName)) continue;
            //         File.Delete(item);
            //     }
            // }

            if (!string.IsNullOrWhiteSpace(settings.outputDataFilePath))
            {
                string dir = Path.GetDirectoryName(settings.outputDataFilePath);
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(settings.outputDataFilePath, memoryStream.ToArray());
            }
            if (!string.IsNullOrWhiteSpace(settings.outputScriptPath))
            {
                Directory.CreateDirectory(settings.outputScriptPath);

                string enumFile = $"{settings.outputScriptPath}/Excel.Enum.g.cs";
                File.WriteAllText(enumFile, ExcelCodeGenerator.GenerateEnumCode(enumDict.Values.ToArray(), settings.namespaceName));
                string dataFile = $"{settings.outputScriptPath}/Excel.Config.g.cs";
                File.WriteAllText(dataFile, ExcelCodeGenerator.GenerateExcelConfigCode(excelList.ToArray(), settings.namespaceName));
                string loaderFile = $"{settings.outputScriptPath}/Excel.Data.g.cs";
                File.WriteAllText(loaderFile, ExcelCodeGenerator.GenerateExcelDataCode(excelList.ToArray(), settings.namespaceName));
            }
        }

        private static void ParseExcelFile(string excelFile, ExcelCommandSettings settings, List<ExcelInfo> excelList, Dictionary<string, ExcelEnumInfo> enumDict, BinaryWriter writer)
        {
            using var stream = File.Open(excelFile, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet();

            Dictionary<string, List<DataTable>> tempDict = new();
            foreach (DataTable dataTable in result.Tables)
            {
                if (dataTable.TableName.StartsWith("#")) continue;
                if (dataTable.TableName == "Enum" || dataTable.TableName.StartsWith("Enum_"))
                {
                    ParseEnum(dataTable, enumDict);
                    continue;
                }

                var tableName = dataTable.TableName.Split("_")[0];
                if (!tempDict.TryGetValue(tableName, out var tbList))
                {
                    tbList = new List<DataTable>();
                    tempDict.Add(tableName, tbList);
                }
                tbList.Add(dataTable);
            }

            // StringBuilder debugBuilder = new StringBuilder();
            foreach (var keyValuePair in tempDict)
            {
                var tableName = keyValuePair.Key;
                var tbList = keyValuePair.Value;

                DataTable keyTb = tbList[0];
                if (keyTb.Rows.Count < DataRow - 1) continue;

                var heads = ParseHeads(keyTb);
                if (heads.Length == 0) continue;

                string excelName = $"{settings.prefixName}{tableName}{settings.suffixName}";
                var typeName = $"{settings.prefixName}{tableName}{settings.suffixName}";
                excelList.Add(new ExcelInfo(settings.namespaceName, tableName, typeName, heads));

                int dataLen = 0;
                foreach (DataTable dataTable in tbList)
                {
                    int rowCount = dataTable.Rows.Count;
                    for (int i = DataRow - 1; i < rowCount; i++)
                    {
                        var cellKeyStr = dataTable.Rows[i][heads[0].Index]?.ToString();
                        if (string.IsNullOrWhiteSpace(cellKeyStr)) continue;
                        dataLen++;
                    }
                }

                writer.Write(dataLen);
                // debugBuilder.AppendLine($"{excelName} : {dataLen}");

                foreach (DataTable dataTable in tbList)
                {
                    int rowCount = dataTable.Rows.Count;
                    for (int i = DataRow - 1; i < rowCount; i++)
                    {
                        var row = dataTable.Rows[i];

                        var cellKeyStr = row[heads[0].Index]?.ToString();
                        if (string.IsNullOrWhiteSpace(cellKeyStr)) continue;

                        StringBuilder sb = new StringBuilder();
                        for (int j = 0; j < heads.Length; j++)
                        {
                            var headInfo = heads[j];

                            // progressAction?.Invoke($"{dataTable.TableName}: {row.ItemArray.Length}, {keyIndex}", 0, 0);
                            var cell = row.ItemArray.Length > headInfo.Index ? row[headInfo.Index] : null;
                            var cellStr = cell?.ToString();

                            if (j == 0) sb.Append($"{headInfo.Key} = {cellStr}");
                            else sb.Append($", {headInfo.Key} = {cellStr}");

                            WriteCellValue(writer, headInfo.WriteType, cell);
                        }

                        // debugBuilder.AppendLine(sb.ToString());
                    }
                }

                // if (!string.IsNullOrWhiteSpace(settings.OutputDebugPath))
                // {
                //     Directory.CreateDirectory(settings.OutputDebugPath);
                //     File.WriteAllText($"{settings.OutputDebugPath}/{excelName}.txt", debugBuilder.ToString());
                // }
            }
        }

        private static void ParseEnum(DataTable dataTable, Dictionary<string, ExcelEnumInfo> enumDict)
        {
            var heads = ParseHeads(dataTable);
            if (heads.Length == 0) return;

            var typeHeadInfo = heads.FirstOrDefault(item => item.Key == "Type");
            var nameHeadInfo = heads.FirstOrDefault(item => item.Key == "Name");
            var valueHeadInfo = heads.FirstOrDefault(item => item.Key == "Value");
            var descHeadInfo = heads.FirstOrDefault(item => item.Key == "Text");

            if (typeHeadInfo == null) throw new Exception($"Excel parse enum error. Can`t find key[Type]!");
            if (nameHeadInfo == null) throw new Exception($"Excel parse enum error. Can`t find key[Name]!");
            if (valueHeadInfo == null) throw new Exception($"Excel parse enum error. Can`t find key[Value]!");
            
            int rowCount = dataTable.Rows.Count;
            for (int i = DataRow - 1; i < rowCount; i++)
            {
                var row = dataTable.Rows[i];
                var typeStr = row.GetCellString(typeHeadInfo.Index);
                var nameStr = row.GetCellString(nameHeadInfo.Index);
                var valueStr = row.GetCellString(valueHeadInfo.Index);
                var descStr = descHeadInfo != null ? row.GetCellString(descHeadInfo.Index) : null;

                if (string.IsNullOrEmpty(typeStr)) continue;
                if (string.IsNullOrEmpty(nameStr)) continue;
                if (string.IsNullOrEmpty(valueStr) || !int.TryParse(valueStr, out var enumValue)) continue;

                if (!enumDict.TryGetValue(typeStr, out var enumInfo))
                {
                    enumInfo = new ExcelEnumInfo(typeStr);
                    enumDict.Add(typeStr, enumInfo);
                }
                enumInfo.Add(nameStr, enumValue, descStr);
            }
        }

        private static ExcelDataHeadInfo[] ParseHeads(DataTable dataTable)
        {
            List<ExcelDataHeadInfo> list = new();
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (UseTypeRow - 1 > 0)
                {
                    var useTypeValue = dataTable.Rows[UseTypeRow - 1][i].ToString();
                    if (string.IsNullOrWhiteSpace(useTypeValue) || !UseTypeHash.Contains(useTypeValue)) continue;
                }
                var keyValue = dataTable.Rows[KeyRow - 1][i]?.ToString();
                var typeValue = dataTable.Rows[TypeRow - 1][i]?.ToString();
                var descValue = dataTable.Rows[DescRow - 1][i]?.ToString();
                if (string.IsNullOrWhiteSpace(keyValue)) continue;
                if (string.IsNullOrWhiteSpace(typeValue)) continue;
                    
                keyValue = keyValue.Trim();
                typeValue = typeValue.Trim();
                if (TypeChangeDict.TryGetValue(typeValue, out var value)) typeValue = value;
                if (SupportTypes.Contains(typeValue))
                {
                    list.Add(new ExcelDataHeadInfo(i, keyValue, typeValue, typeValue, descValue));
                }
                else if (typeValue.StartsWith("enum"))
                {
                    var arr = typeValue.Split("=");
                    if (arr.Length != 2) throw new Exception($"Try parse enum type error. type: {typeValue}");
                    list.Add(new ExcelDataHeadInfo(i, keyValue, arr[1], "enum", descValue));
                }

            }
            return list.ToArray();
        }

        private static void WriteCellValue(BinaryWriter writer, string type, object value)
        {
            string cellStr = value?.ToString() ?? string.Empty;

            if (Writers.TryGetValue(type, out var action))
            {
                action(writer, cellStr);
            }
            else
            {
                throw new Exception($"ExcelCommand Unknown type {type}");
            }
        }

        private static string[] GetExcelFiles(string directory)
        {
            List<string> tmpList = new();
            foreach (var ex in ExcelExes)
            {
                var arr = Directory.GetFiles(directory, $"*{ex}", SearchOption.AllDirectories);
                foreach (var file in arr)
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName.StartsWith("~$")) continue;
                    tmpList.Add(file);
                }
            }
            return tmpList.ToArray();
        }

    }
}
