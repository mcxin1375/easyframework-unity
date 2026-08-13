using System.Data;

namespace EasyFramework.Editor
{
    public static class ExcelDataReaderExtension
    {
        public static object GetCellObject(this DataRow dataRow, int index)
        {
            return dataRow.ItemArray.Length > index ? dataRow[index] : null;
        }
        public static string GetCellString(this DataRow dataRow, int index)
        {
            var obj = dataRow.ItemArray.Length > index ? dataRow[index] : null;
            return obj?.ToString() ?? string.Empty;
        }
    }
}
