
namespace EasyFramework.Editor
{
    public class ExcelInfo
    {
        public readonly string Namespace;
        public readonly string TableName;
        public readonly string TypeName;
        public readonly ExcelDataHeadInfo[] Heads;

        public string KeyType => Heads[0].Type;
        public string KeyValue => Heads[0].Key;

        public ExcelInfo(string nameSpace, string tableName, string typeName, ExcelDataHeadInfo[] heads)
        {
            Namespace = nameSpace;
            TableName = tableName;
            TypeName = typeName;
            Heads = heads;
        }
    }
}
